using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    // basis variables
    public static float LINEAR_COEF = 0.7f;
    public static float GRAVITY = -700f;
    public static float ANGULAR_ACC = 20f;

    // trigger system variables
    public static float teleport_trigger_cooldown = 1f;

    // battle system variables
    public static double line_radius = 1f;
    public static int game_difficult = 3;
    public static float default_notice_range = 1.9f;

    // balance variables
    public static int player_heal_cost = 10;
    public static int player_teleport_cost = 20;
    public static int player_rewind_cost = 10;
    public static int enemy_power_price = 22;

    public static float player_heal_cooldown = 0.02f;
    public static float player_teleport_cooldown = 0.3f;
    public static float player_position_rewind_cooldown = 0.01f;

    public static float player_teleport_distance = 100f;

    // DataRecorder variables
    public static bool is_writing = true;

    // camera's variables
    public static float camera_speed = 7f;
    public static float camera_shaking_duration = 0.3f;
    public static float camera_shaking_amplitude = 0f;
    public static float camera_force_soft_coef = 1f;
    public static float camera_distance_offset = 7.5f;
    public static float camera_critical_distance = 30f;


    // player's variables 
    public static int player_max_hp = 100;
    public static int player_max_energy = 1000;
    public static float player_max_speed = 7f;

    public static int player_trace_max_length = 250;
    public static int player_position_rewind_offset = 20;
    public static int player_tracing_step = 5;

    public static float player_parry_window_duration = 0.2f;

    // enemies' varibles
    // TODO: write max_hp and max_energy
    public static float melee_max_speed = 6f;
    public static float melee_enemy_push_force = 5f;
    public static float push_machine_push_force = 30f;


    // standard weapons variables
    // TODO: player_weapon_cooldown should be list
    // TODO: add push forces to enemies
    public static int player_weapon_damage = 35;
    public static float player_weapon_cooldown = 0.3f;
    public static float player_weapon_push_force = 5f;

    public static int samurai_weapon_damage = 2;
    public static List<float> samurai_weapon_cooldown = new List<float>() { 0.0f, 0.5f, 0.0f, 0.4f };

    public static int swordsman_weapons_damage = 4;
    public static List<float> swordman_weapon_cooldown = new List<float>() { 0.0f, 0.8f, 0.0f, 0.9f };

    public static int push_machine_weapon_damage = 0;
    public static List<float> push_machine_weapon_cooldown = new List<float>() { 0.0f, 0.2f, 0.0f, 0.8f };

    // sound variables
    public static int music_max_enemy_amount = 5;
}