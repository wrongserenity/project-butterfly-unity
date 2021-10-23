using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    // basis variables
    public static float LINEAR_COEF = 0.7f;
    public static float GRAVITY = -700f;
    public static float ANGULAR_ACC = 20f;

    // graphics variables
    public static float xiton_particle_life_time = 2f;
    public static int short_distance_teleport_circle_count = 5;
    public static float short_distance_teleport_circle_lifetime = 0.5f;
    public static float short_distance_teleport_circle_timestep = 0.05f;
    public static float enemies_deprivation_ready_ripple_tick_time = 0.5f;
    public static float enemies_damaged_animation_tick_time = 0.1f;
    public static float enemies_damaged_animation_duration = 0.5f;

    public static float energy_sphere_life_time = 5f;
    public static float energy_sphere_no_move_time = 0.5f;
    public static float energy_sphere_max_collect_distance = 10f;
    public static float energy_sphere_min_collect_distance = 1f;
    public static float energy_sphere_speed = 15f;

    // level objects variables
    public static float glowing_object_step_time_charge = 0.1f;
    public static float glowing_object_step_charge = 0.02f;
    public static float glowing_object_step_using = 0.01f;
    public static float glowing_object_recharge_delay = 3f;

    public static float gravity_trigger_tick_cooldown = 0.05f;
    public static float gravity_trigger_push_force = 3f;

    // trigger system variables
    public static float teleport_trigger_cooldown = 1f;

    // battle system variables
    public static double line_radius = 1f;
    public static int game_difficult = 3;
    public static float default_notice_range = 7f;
    public static float default_forget_range = 20f;

    // balance variables
    public static int player_heal_cost = 10;
    public static int player_teleport_cost = 20;
    public static int player_rewind_cost = 10;
    public static int enemy_power_price = 22;

    public static float player_heal_cooldown = 0.02f;
    public static float player_teleport_cooldown = 0.3f;
    public static float player_position_rewind_cooldown = 0.01f;
    public static float player_xiton_charge_cooldown = 0.2f;
    public static float player_deprivation_cooldown = 0.5f;

    public static float player_parry_damage_boost_time = 0.5f;

    public static float player_teleport_distance = 5f;

    // DataRecorder variables
    public static bool is_writing = true;

    // camera's variables
    public static float camera_speed = 7f;
    public static float camera_shaking_duration = 0.3f;
    public static float camera_shaking_amplitude = 0f;
    public static float camera_force_soft_coef = 1f;
    public static float camera_distance_offset = 13.5f;
    public static float camera_critical_distance = 30f;


    // player's variables 
    public static int player_max_hp = 100;
    public static int player_max_energy = 1000;
    public static float player_max_speed = 7f;
    public static int player_max_xiton_charge = 100;

    public static int player_trace_max_length = 50;
    public static int player_position_rewind_offset = 20;
    public static int player_tracing_step = 5;

    public static float player_parry_window_duration = 0.2f;
    public static float player_parry_damage_immune_duration = 1f;

    // enemies' varibles
    // TODO: write max_hp and max_energy
    public static float melee_max_speed = 6f;
    public static float melee_enemy_push_force = 5f;
    public static float push_machine_push_force = 70f;

    public static float deprivateble_hp_percent = 0.7f;


    // standard weapons variables
    // TODO: player_weapon_cooldown should be list
    // TODO: add push forces to enemies
    public static int player_weapon_damage = 20;
    public static float player_weapon_cooldown = 0.3f;
    public static float player_weapon_push_force = 5f;
    public static int player_weapon_xiton_damage_scaling_cost = 10;
    public static float player_weapon_xiton_scaling = 5f;
    public static float player_weapon_parry_scaling = 4f;

    public static int samurai_weapon_damage = 20; // 2
    public static List<float> samurai_weapon_cooldown = new List<float>() { 0.0f, 0.3f, 0.0f, 0.2f };

    public static int swordsman_weapons_damage = 40; // 4
    public static List<float> swordman_weapon_cooldown = new List<float>() { 0.0f, 0.4f, 0.0f, 0.3f };

    public static int push_machine_weapon_damage = 0;
    public static List<float> push_machine_weapon_cooldown = new List<float>() { 0.0f, 0.2f, 0.0f, 0.8f };

    // sound variables
    public static int music_max_enemy_amount = 5;

    // special weapons variables
    public static float gravity_bomb_throw_distance = 10f;
    public static float gravity_bomb_impact_radius = 5f;
    public static float gravity_bomb_impulse_force = 5f;
    public static float gravity_bomb_impact_time_step = 0.05f;
    public static float gravity_bomb_impact_duration = 4f;
    public static int gravity_bomb_damage = 3;

    public static float firethrower_cooldown = 0.1f;
    public static int firethrower_damage = 5;
    public static float firethrower_push_force = 20f;
    public static float firethrower_fuel_duration_sec = 4f;
}
