import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.ticker as mtick
import seaborn as sns
import os

csv_file_path = 'Assets\\TowerTests\\Cannon_TestResults_20250407_004307.csv'
plot_directory = 'tower_plots'
target_tower_name = "Cannon" # <--- Change this to the tower you want to analyze

def analyze_tower_performance(file_path, specific_tower_name):
    main_plot_dir = plot_directory
    single_tower_plot_dir = os.path.join(main_plot_dir, specific_tower_name)

    for dir_path in [main_plot_dir, single_tower_plot_dir]:
        if not os.path.exists(dir_path):
            try:
                os.makedirs(dir_path)
                print(f"Created directory for plots: '{dir_path}'")
            except OSError as e:
                 print(f"Error creating directory {dir_path}: {e}")
                 return

    try:
        df = pd.read_csv(file_path, na_values=['Inf'])

        numeric_cols = [
            'Level', 'Cost', 'DPS_Avg', 'TotalDamage', 'Kills',
            'EffectiveDPS', 'OverkillPercentage', 'PeakDPS', 'Accuracy',
            'AOEHits_Avg', 'EffectProcRate', 'HP', 'MaxHP', 'TimeAlive',
            'OverkillDamage', 'AttackConsistency', 'TowerRating'
        ]
        for col in numeric_cols:
            if col in df.columns:
                df[col] = pd.to_numeric(df[col], errors='coerce')

        print(f"--- Analysis Results for: {file_path} ---")

        if df.empty:
            print("No valid data found in the CSV file after initial processing.")
            return

        # --- Single Tower Analysis ---
        print(f"\n--- Detailed Analysis for Tower: {specific_tower_name} ---")
        df_single_tower = df[df['TowerName'] == specific_tower_name].sort_values(by='Level').copy()

        if df_single_tower.empty:
            print(f"No data found for tower '{specific_tower_name}' in the CSV.")
            return

        if df_single_tower['Level'].isnull().any():
             print("Warning: Some entries for this tower have missing Level information. Results may be incomplete.")
             df_single_tower.dropna(subset=['Level'], inplace=True)
             df_single_tower['Level'] = df_single_tower['Level'].astype(int)
             df_single_tower.sort_values(by='Level', inplace=True)

        if df_single_tower.shape[0] < 2:
             print("Need data for at least two levels of the tower to show progression graphs.")

        # Calculate Cost-Effectiveness Metrics
        df_single_tower['EffDPS_Per_Cost'] = np.where(
            df_single_tower['Cost'] > 0,
            df_single_tower['EffectiveDPS'] / df_single_tower['Cost'] * 1000, # DPS per 1000 cost units
            0
        )
        df_single_tower['Rating_Per_Cost'] = np.where(
            df_single_tower['Cost'] > 0,
            df_single_tower['TowerRating'] / df_single_tower['Cost'] * 100, # Rating points per 100 cost units
            0
        )

        print("\nKey Metrics per Level:")
        cols_to_show = ['Level', 'Cost', 'TowerRating', 'EffectiveDPS', 'EffDPS_Per_Cost', 'Rating_Per_Cost', 'OverkillPercentage', 'Accuracy']
        print(df_single_tower[cols_to_show].round(2).to_string(index=False))

        # Find Best Value Levels
        if 'EffDPS_Per_Cost' in df_single_tower.columns and df_single_tower['EffDPS_Per_Cost'].notna().any():
             best_dps_eff_level = df_single_tower.loc[df_single_tower['EffDPS_Per_Cost'].idxmax()]
             print(f"\nBest DPS Value (Eff. DPS per 1k Cost): Level {int(best_dps_eff_level['Level'])} ({best_dps_eff_level['EffDPS_Per_Cost']:.2f})")

        if 'Rating_Per_Cost' in df_single_tower.columns and df_single_tower['Rating_Per_Cost'].notna().any():
             best_rating_eff_level = df_single_tower.loc[df_single_tower['Rating_Per_Cost'].idxmax()]
             print(f"Best Rating Value (Rating per 100 Cost): Level {int(best_rating_eff_level['Level'])} ({best_rating_eff_level['Rating_Per_Cost']:.2f})")

        # --- Generate Single Tower Plots ---
        print(f"\n--- Generating Plots for {specific_tower_name} ---")
        plot_paths_single = []
        levels = df_single_tower['Level'].astype(int) # Use integer levels for ticks

        try:
            # Plot 1: Effective DPS Progression
            if 'EffectiveDPS' in df_single_tower.columns and df_single_tower['EffectiveDPS'].notna().any():
                plt.figure(figsize=(9, 5))
                sns.lineplot(data=df_single_tower, x='Level', y='EffectiveDPS', marker='o', errorbar=None)
                plt.title(f'{specific_tower_name}: Effective DPS vs. Level')
                plt.xlabel('Tower Level')
                plt.ylabel('Effective DPS')
                plt.xticks(levels)
                plt.grid(axis='y', linestyle='--', alpha=0.7)
                plt.tight_layout()
                save_path = os.path.join(single_tower_plot_dir, f'{specific_tower_name}_EffDPS_vs_Level.png')
                plt.savefig(save_path)
                plt.close()
                plot_paths_single.append(save_path)

            # Plot 2: Tower Rating Progression
            if 'TowerRating' in df_single_tower.columns and df_single_tower['TowerRating'].notna().any():
                plt.figure(figsize=(9, 5))
                sns.barplot(data=df_single_tower, x='Level', y='TowerRating', palette='viridis', width=0.6)
                plt.title(f'{specific_tower_name}: Tower Rating vs. Level')
                plt.xlabel('Tower Level')
                plt.ylabel('Tower Rating (0-100)')
                plt.ylim(0, 105) # Assuming rating is 0-100
                plt.xticks(range(len(levels)), levels) # Correct ticks for barplot
                plt.grid(axis='y', linestyle='--', alpha=0.7)
                plt.tight_layout()
                save_path = os.path.join(single_tower_plot_dir, f'{specific_tower_name}_Rating_vs_Level.png')
                plt.savefig(save_path)
                plt.close()
                plot_paths_single.append(save_path)

            # Plot 3: Overkill Percentage Progression
            if 'OverkillPercentage' in df_single_tower.columns and df_single_tower['OverkillPercentage'].notna().any():
                plt.figure(figsize=(9, 5))
                sns.lineplot(data=df_single_tower, x='Level', y='OverkillPercentage', marker='o', color='red', errorbar=None)
                plt.title(f'{specific_tower_name}: Overkill Percentage vs. Level')
                plt.xlabel('Tower Level')
                plt.ylabel('Overkill Percentage')
                plt.gca().yaxis.set_major_formatter(mtick.PercentFormatter(xmax=1.0))
                plt.xticks(levels)
                plt.grid(axis='y', linestyle='--', alpha=0.7)
                plt.tight_layout()
                save_path = os.path.join(single_tower_plot_dir, f'{specific_tower_name}_Overkill_vs_Level.png')
                plt.savefig(save_path)
                plt.close()
                plot_paths_single.append(save_path)

            # Plot 4: Cost Effectiveness (DPS/Cost)
            if 'EffDPS_Per_Cost' in df_single_tower.columns and df_single_tower['EffDPS_Per_Cost'].notna().any():
                 plt.figure(figsize=(9, 5))
                 sns.barplot(data=df_single_tower, x='Level', y='EffDPS_Per_Cost', palette='magma', width=0.6)
                 plt.title(f'{specific_tower_name}: Effective DPS per 1000 Cost vs. Level')
                 plt.xlabel('Tower Level')
                 plt.ylabel('Effective DPS / 1000 Cost')
                 plt.xticks(range(len(levels)), levels)
                 plt.grid(axis='y', linestyle='--', alpha=0.7)
                 plt.tight_layout()
                 save_path = os.path.join(single_tower_plot_dir, f'{specific_tower_name}_EffDPS_Per_Cost_vs_Level.png')
                 plt.savefig(save_path)
                 plt.close()
                 plot_paths_single.append(save_path)

            # Plot 5: Cost Effectiveness (Rating/Cost)
            if 'Rating_Per_Cost' in df_single_tower.columns and df_single_tower['Rating_Per_Cost'].notna().any():
                 plt.figure(figsize=(9, 5))
                 sns.barplot(data=df_single_tower, x='Level', y='Rating_Per_Cost', palette='plasma', width=0.6)
                 plt.title(f'{specific_tower_name}: Rating per 100 Cost vs. Level')
                 plt.xlabel('Tower Level')
                 plt.ylabel('Rating / 100 Cost')
                 plt.xticks(range(len(levels)), levels)
                 plt.grid(axis='y', linestyle='--', alpha=0.7)
                 plt.tight_layout()
                 save_path = os.path.join(single_tower_plot_dir, f'{specific_tower_name}_Rating_Per_Cost_vs_Level.png')
                 plt.savefig(save_path)
                 plt.close()
                 plot_paths_single.append(save_path)

            # Plot 6: Accuracy and Consistency Progression
            metrics_to_plot = []
            if 'Accuracy' in df_single_tower.columns and df_single_tower['Accuracy'].notna().any():
                metrics_to_plot.append('Accuracy')
            if 'AttackConsistency' in df_single_tower.columns and df_single_tower['AttackConsistency'].notna().any():
                 metrics_to_plot.append('AttackConsistency')

            if metrics_to_plot:
                plt.figure(figsize=(9, 5))
                for metric in metrics_to_plot:
                    sns.lineplot(data=df_single_tower, x='Level', y=metric, marker='o', label=metric, errorbar=None)
                plt.title(f'{specific_tower_name}: Accuracy & Consistency vs. Level')
                plt.xlabel('Tower Level')
                plt.ylabel('Value (0-1 Scale)')
                plt.ylim(-0.05, 1.05)
                plt.xticks(levels)
                plt.legend()
                plt.grid(axis='y', linestyle='--', alpha=0.7)
                plt.tight_layout()
                save_path = os.path.join(single_tower_plot_dir, f'{specific_tower_name}_Accuracy_Consistency_vs_Level.png')
                plt.savefig(save_path)
                plt.close()
                plot_paths_single.append(save_path)

        except Exception as e:
            print(f"An error occurred during single tower plotting: {e}")

        if plot_paths_single:
            print(f"\nSingle tower plots saved to '{single_tower_plot_dir}':")
        else:
            print("\nNo single tower plots were generated.")

        print("\n--- Analysis Complete ---")

    except FileNotFoundError:
        print(f"Error: CSV file not found at '{file_path}'")
    except pd.errors.EmptyDataError:
        print(f"Error: CSV file at '{file_path}' is empty.")
    except Exception as e:
        print(f"An error occurred during analysis: {e}")

if __name__ == "__main__":
    analyze_tower_performance(csv_file_path, target_tower_name)