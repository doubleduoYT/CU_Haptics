using System;
using System.Collections.Generic;
using UnityEngine;

public static class HGHaptics
{
    public enum OutputTarget
    {
        Off,
        Phone,
        Controller,
        ControllerWithoutRumble
    }

    private enum Cue
    {
        Direction,
        UiTick,
        Confirm,
        Jump,
        AttackPress,
        Throw,
        ThrowCharge,
        ThrowRelease,
        MeleeHit,
        GunPistol,
        GunRifle,
        GunShotgun,
        DamageLight,
        DamageMedium,
        DamageHeavy,
        LandingLight,
        LandingHeavy,
        JumpPad,
        Impact,
        Explosion,
        Heartbeat,
        Earthquake,
        EarthquakeDebris,
        Radiation,
        GeyserWarning,
        GeyserBurst,
        GeyserFlow,
        MineArmed,
        SoundCannonCharge,
        SoundCannonBlast,
        GunSafety,
        GunLoad,
        GunUnload,
        GunTrigger,
        GunRack,
        GunJam,
        LockpickResistance,
        LockpickBreak,
        LockpickSuccess,
        DislocationHit,
        DislocationSuccess,
        AmputationCut,
        AmputationComplete,
        WorldExplosion,
        FallingHazardStart,
        FallingHazardImpact,
        MinigameOpen,
        MinigameClose,
        MinigameGrab,
        KeypadPress,
        BandageWrap,
        ShrapnelPull,
        ShrapnelSlip,
        SyringePuncture,
        SyringeInject,
        SyringeSlip,
        SelfHarmTap,
        SelfHarmCut,
        AEDPad,
        AEDShock,
        DefibDial,
        DefibCharge,
        DefibShock,
        HandCrank,
        SelfDestructPulse,
        Death
    }

    internal sealed class Pattern
    {
        public readonly long[] timings;
        public readonly int[] amplitudes;
        public readonly int priority;
        public readonly float cooldown;
        public readonly float controllerLeftScale;
        public readonly float controllerRightScale;

        public Pattern(
            long[] timings,
            int[] amplitudes,
            int priority,
            float cooldown,
            float controllerLeftScale = 0.82f,
            float controllerRightScale = 1f)
        {
            this.timings = timings;
            this.amplitudes = amplitudes;
            this.priority = priority;
            this.cooldown = cooldown;
            this.controllerLeftScale = controllerLeftScale;
            this.controllerRightScale = controllerRightScale;
        }

        public float DurationSeconds
        {
            get
            {
                long duration = 0;
                for (int i = 0; i < timings.Length; i++)
                    duration += timings[i];
                return duration * 0.001f;
            }
        }
    }

    private const string EnabledKey = "hg_haptics_enabled";
    private const string IntensityKey = "hg_haptics_intensity";

    private static readonly Dictionary<Cue, float> LastPlayed =
        new Dictionary<Cue, float>();

    private static float protectedUntil;
    private static int protectedPriority;

    private static float nextRadiationTick;
    private static float nextGeyserFlowTick;
    private static float nextSoundCannonPulse;
    private static float nextLockpickPulse;
    private static float nextAmputationPulse;
    private static int lastThrowChargeStep;
    private static float nextShrapnelPulse;
    private static float nextSyringePulse;
    private static float nextSelfHarmCutPulse;
    private static float nextDefibDialPulse;
    private static float nextHandCrankPulse;
    private static float lastExplosionAt = -10f;
    private static Vector2 lastExplosionPosition = new Vector2(99999f, 99999f);
    private static float suppressDeathUntil;

    private static readonly Pattern DirectionPattern = new Pattern(
        new long[] { 0, 8 },
        new int[] { 0, 38 },
        0, 0.055f, 0.45f, 0.60f);

    private static readonly Pattern UiTickPattern = new Pattern(
        new long[] { 0, 10 },
        new int[] { 0, 58 },
        1, 0.045f, 0.50f, 0.68f);

    private static readonly Pattern ConfirmPattern = new Pattern(
        new long[] { 0, 11, 34, 18 },
        new int[] { 0, 82, 0, 142 },
        2, 0.12f, 0.62f, 0.80f);

    private static readonly Pattern JumpPattern = new Pattern(
        new long[] { 0, 12, 10, 24 },
        new int[] { 0, 72, 0, 156 },
        2, 0.09f, 0.62f, 0.82f);

    private static readonly Pattern AttackPressPattern = new Pattern(
        new long[] { 0, 10, 8, 19 },
        new int[] { 0, 88, 0, 168 },
        2, 0.075f, 0.72f, 0.90f);

    private static readonly Pattern ThrowPattern = new Pattern(
        new long[] { 0, 14, 11, 30 },
        new int[] { 0, 100, 0, 184 },
        3, 0.12f, 0.78f, 0.96f);

    private static readonly Pattern ThrowChargePattern = new Pattern(
        new long[] { 0, 8, 8, 13 },
        new int[] { 0, 54, 0, 112 },
        2, 0.035f, 0.58f, 0.86f);

    private static readonly Pattern ThrowReleasePattern = new Pattern(
        new long[] { 0, 13, 8, 27, 15, 25 },
        new int[] { 0, 104, 0, 214, 0, 82 },
        5, 0.10f, 0.88f, 1f);

    private static readonly Pattern MeleePattern = new Pattern(
        new long[] { 0, 10, 8, 25, 14, 18 },
        new int[] { 0, 92, 0, 214, 0, 78 },
        4, 0.10f, 0.88f, 1f);

    private static readonly Pattern PistolPattern = new Pattern(
        new long[] { 0, 12, 9, 28, 24, 18 },
        new int[] { 0, 150, 0, 232, 0, 72 },
        5, 0.07f, 0.92f, 0.72f);

    private static readonly Pattern RiflePattern = new Pattern(
        new long[] { 0, 9, 7, 18, 17, 31 },
        new int[] { 0, 172, 0, 246, 0, 98 },
        5, 0.055f, 0.96f, 0.78f);

    private static readonly Pattern ShotgunPattern = new Pattern(
        new long[] { 0, 16, 11, 47, 20, 72 },
        new int[] { 0, 216, 0, 255, 0, 128 },
        7, 0.16f, 1f, 0.82f);

    private static readonly Pattern DamageLightPattern = new Pattern(
        new long[] { 0, 18, 10, 20 },
        new int[] { 0, 100, 0, 154 },
        5, 0.14f, 0.74f, 0.88f);

    private static readonly Pattern DamageMediumPattern = new Pattern(
        new long[] { 0, 22, 10, 34, 18, 30 },
        new int[] { 0, 146, 0, 220, 0, 94 },
        6, 0.20f, 0.90f, 1f);

    private static readonly Pattern DamageHeavyPattern = new Pattern(
        new long[] { 0, 28, 11, 48, 18, 78 },
        new int[] { 0, 224, 0, 255, 0, 142 },
        8, 0.32f, 1f, 0.90f);

    private static readonly Pattern LandingLightPattern = new Pattern(
        new long[] { 0, 15, 10, 20 },
        new int[] { 0, 82, 0, 136 },
        3, 0.16f, 0.78f, 0.88f);

    private static readonly Pattern LandingHeavyPattern = new Pattern(
        new long[] { 0, 24, 10, 44, 22, 45 },
        new int[] { 0, 172, 0, 238, 0, 106 },
        6, 0.28f, 1f, 0.86f);

    private static readonly Pattern JumpPadPattern = new Pattern(
        new long[] { 0, 15, 9, 27, 9, 52 },
        new int[] { 0, 72, 0, 154, 0, 236 },
        7, 0.34f, 0.72f, 1f);

    private static readonly Pattern ImpactPattern = new Pattern(
        new long[] { 0, 25, 13, 48, 20, 62 },
        new int[] { 0, 160, 0, 230, 0, 112 },
        6, 0.25f, 1f, 0.82f);

    private static readonly Pattern ExplosionPattern = new Pattern(
        new long[] { 0, 35, 16, 68, 22, 118 },
        new int[] { 0, 184, 0, 255, 0, 126 },
        9, 0.48f, 1f, 0.78f);

    private static readonly Pattern HeartbeatPattern = new Pattern(
        new long[] { 0, 42, 82, 62 },
        new int[] { 0, 164, 0, 220 },
        4, 0.50f, 0.92f, 0.76f);

    private static readonly Pattern EarthquakeLowPattern = new Pattern(
        new long[] { 0, 42, 52, 36 },
        new int[] { 0, 44, 0, 72 },
        3, 0.20f, 0.94f, 0.48f);

    private static readonly Pattern EarthquakeMediumPattern = new Pattern(
        new long[] { 0, 54, 25, 62, 39, 42 },
        new int[] { 0, 82, 0, 128, 0, 68 },
        5, 0.17f, 1f, 0.58f);

    private static readonly Pattern EarthquakeHeavyPattern = new Pattern(
        new long[] { 0, 66, 20, 88, 28, 58 },
        new int[] { 0, 132, 0, 188, 0, 104 },
        7, 0.145f, 1f, 0.68f);

    private static readonly Pattern EarthquakeDebrisPattern = new Pattern(
        new long[] { 0, 16, 9, 34, 16, 22 },
        new int[] { 0, 118, 0, 208, 0, 82 },
        7, 0.11f, 0.80f, 1f);

    private static readonly Pattern RadiationPattern = new Pattern(
        new long[] { 0, 9, 7, 13 },
        new int[] { 0, 86, 0, 142 },
        4, 0.08f, 0.48f, 0.94f);

    private static readonly Pattern RadiationExposedPattern = new Pattern(
        new long[] { 0, 17, 10, 26, 14, 19 },
        new int[] { 0, 116, 0, 184, 0, 92 },
        6, 0.15f, 0.66f, 1f);

    private static readonly Pattern GeyserWarningPattern = new Pattern(
        new long[] { 0, 20, 26, 24, 20, 31, 14, 42 },
        new int[] { 0, 46, 0, 78, 0, 126, 0, 178 },
        5, 0.70f, 1f, 0.60f);

    private static readonly Pattern GeyserBurstPattern = new Pattern(
        new long[] { 0, 30, 12, 58, 18, 74 },
        new int[] { 0, 144, 0, 232, 0, 112 },
        8, 0.55f, 1f, 0.82f);

    private static readonly Pattern GeyserFlowPattern = new Pattern(
        new long[] { 0, 28, 36, 20 },
        new int[] { 0, 62, 0, 94 },
        3, 0.19f, 0.92f, 0.50f);

    private static readonly Pattern MineArmedPattern = new Pattern(
        new long[] { 0, 13, 15, 8 },
        new int[] { 0, 196, 0, 88 },
        7, 0.65f, 0.62f, 1f);

    private static readonly Pattern SoundCannonChargePattern = new Pattern(
        new long[] { 0, 22, 35, 18 },
        new int[] { 0, 76, 0, 118 },
        5, 0.10f, 0.78f, 1f);

    private static readonly Pattern SoundCannonBlastPattern = new Pattern(
        new long[] { 0, 42, 13, 92, 24, 136 },
        new int[] { 0, 210, 0, 255, 0, 138 },
        10, 0.75f, 1f, 0.96f);

    private static readonly Pattern GunSafetyPattern = new Pattern(
        new long[] { 0, 8, 18, 8 },
        new int[] { 0, 72, 0, 118 },
        2, 0.08f, 0.45f, 0.86f);

    private static readonly Pattern GunLoadPattern = new Pattern(
        new long[] { 0, 12, 20, 18 },
        new int[] { 0, 86, 0, 156 },
        3, 0.10f, 0.72f, 0.92f);

    private static readonly Pattern GunUnloadPattern = new Pattern(
        new long[] { 0, 17, 15, 10 },
        new int[] { 0, 122, 0, 66 },
        3, 0.10f, 0.76f, 0.68f);

    private static readonly Pattern GunTriggerPattern = new Pattern(
        new long[] { 0, 7 },
        new int[] { 0, 90 },
        2, 0.055f, 0.38f, 0.78f);

    private static readonly Pattern GunDryTriggerPattern = new Pattern(
        new long[] { 0, 10, 13, 7 },
        new int[] { 0, 122, 0, 58 },
        3, 0.08f, 0.46f, 0.92f);

    private static readonly Pattern GunRackPattern = new Pattern(
        new long[] { 0, 18, 12, 31 },
        new int[] { 0, 76, 0, 174 },
        4, 0.13f, 0.90f, 0.82f);

    private static readonly Pattern GunJamPattern = new Pattern(
        new long[] { 0, 20, 8, 13 },
        new int[] { 0, 116, 0, 54 },
        5, 0.18f, 0.86f, 0.50f);

    private static readonly Pattern LockpickResistancePattern = new Pattern(
        new long[] { 0, 13, 12, 11 },
        new int[] { 0, 58, 0, 102 },
        2, 0.075f, 0.56f, 0.82f);

    private static readonly Pattern LockpickBreakPattern = new Pattern(
        new long[] { 0, 14, 8, 31 },
        new int[] { 0, 176, 0, 92 },
        6, 0.28f, 0.72f, 1f);

    private static readonly Pattern LockpickSuccessPattern = new Pattern(
        new long[] { 0, 10, 23, 22 },
        new int[] { 0, 82, 0, 172 },
        5, 0.32f, 0.72f, 0.92f);

    private static readonly Pattern DislocationHitPattern = new Pattern(
        new long[] { 0, 20, 9, 36 },
        new int[] { 0, 132, 0, 212 },
        7, 0.16f, 1f, 0.78f);

    private static readonly Pattern DislocationSuccessPattern = new Pattern(
        new long[] { 0, 25, 16, 38 },
        new int[] { 0, 104, 0, 184 },
        6, 0.45f, 1f, 0.72f);

    private static readonly Pattern AmputationCutPattern = new Pattern(
        new long[] { 0, 16, 12, 14 },
        new int[] { 0, 54, 0, 86 },
        3, 0.08f, 0.78f, 0.92f);

    private static readonly Pattern AmputationCompletePattern = new Pattern(
        new long[] { 0, 28, 14, 52, 24, 42 },
        new int[] { 0, 146, 0, 220, 0, 82 },
        8, 0.80f, 1f, 0.82f);


    
    
    
    private static readonly Pattern WorldExplosionPattern = new Pattern(
        new long[] { 0, 28, 9, 54, 15, 105, 28, 76 },
        new int[] { 0, 210, 0, 255, 0, 182, 0, 92 },
        10, 0.10f, 1f, 0.86f);

    private static readonly Pattern FallingHazardStartPattern = new Pattern(
        new long[] { 0, 18, 30, 15, 42, 21 },
        new int[] { 0, 48, 0, 76, 0, 112 },
        4, 0.28f, 0.92f, 0.54f);

    private static readonly Pattern FallingHazardImpactPattern = new Pattern(
        new long[] { 0, 24, 8, 48, 16, 72 },
        new int[] { 0, 176, 0, 244, 0, 112 },
        8, 0.13f, 1f, 0.82f);

    private static readonly Pattern MinigameOpenPattern = new Pattern(
        new long[] { 0, 8 }, new int[] { 0, 46 }, 1, 0.10f);
    private static readonly Pattern MinigameClosePattern = new Pattern(
        new long[] { 0, 9, 18, 15 }, new int[] { 0, 58, 0, 96 }, 2, 0.14f);
    private static readonly Pattern MinigameGrabPattern = new Pattern(
        new long[] { 0, 9 }, new int[] { 0, 72 }, 2, 0.055f);
    private static readonly Pattern KeypadPressPattern = new Pattern(
        new long[] { 0, 11 }, new int[] { 0, 84 }, 2, 0.045f);
    private static readonly Pattern BandageWrapPattern = new Pattern(
        new long[] { 0, 12, 12, 17 }, new int[] { 0, 44, 0, 78 }, 2, 0.055f);
    private static readonly Pattern ShrapnelPullPattern = new Pattern(
        new long[] { 0, 11, 9, 12 }, new int[] { 0, 42, 0, 72 }, 3, 0.055f);
    private static readonly Pattern ShrapnelSlipPattern = new Pattern(
        new long[] { 0, 13, 6, 28 }, new int[] { 0, 158, 0, 84 }, 6, 0.22f);
    private static readonly Pattern SyringePuncturePattern = new Pattern(
        new long[] { 0, 13, 10, 8 }, new int[] { 0, 126, 0, 54 }, 5, 0.20f);
    private static readonly Pattern SyringeInjectPattern = new Pattern(
        new long[] { 0, 9 }, new int[] { 0, 38 }, 2, 0.045f);
    private static readonly Pattern SyringeSlipPattern = new Pattern(
        new long[] { 0, 15, 7, 31 }, new int[] { 0, 184, 0, 104 }, 7, 0.30f);
    private static readonly Pattern SelfHarmTapPattern = new Pattern(
        new long[] { 0, 10, 7, 12 }, new int[] { 0, 74, 0, 120 }, 4, 0.06f);
    private static readonly Pattern SelfHarmCutPattern = new Pattern(
        new long[] { 0, 17, 8, 22 }, new int[] { 0, 92, 0, 138 }, 5, 0.065f);
    private static readonly Pattern AEDPadPattern = new Pattern(
        new long[] { 0, 10, 18, 14 }, new int[] { 0, 62, 0, 114 }, 3, 0.10f);
    private static readonly Pattern AEDShockPattern = new Pattern(
        new long[] { 0, 31, 8, 62, 18, 84 }, new int[] { 0, 224, 0, 255, 0, 112 }, 10, 0.60f);
    private static readonly Pattern DefibDialPattern = new Pattern(
        new long[] { 0, 7 }, new int[] { 0, 46 }, 1, 0.035f);
    private static readonly Pattern DefibChargePattern = new Pattern(
        new long[] { 0, 12, 18, 24 }, new int[] { 0, 88, 0, 150 }, 4, 0.18f);
    private static readonly Pattern DefibShockPattern = new Pattern(
        new long[] { 0, 34, 8, 72, 18, 96 }, new int[] { 0, 218, 0, 255, 0, 128 }, 10, 0.62f);
    private static readonly Pattern HandCrankPattern = new Pattern(
        new long[] { 0, 8, 10, 8 }, new int[] { 0, 42, 0, 66 }, 2, 0.04f);
    private static readonly Pattern SelfDestructPulsePattern = new Pattern(
        new long[] { 0, 16, 8, 10 }, new int[] { 0, 92, 0, 54 }, 6, 0.025f);

    private static readonly Pattern DeathPattern = new Pattern(
        new long[] { 0, 82, 28, 122, 38, 184 },
        new int[] { 0, 242, 0, 180, 0, 92 },
        10, 1.20f, 1f, 0.90f);

    public static event Action SettingChanged;

    public static bool Enabled
    {
        get
        {
            if (!PlayerPrefs.HasKey(EnabledKey))
                PlayerPrefs.SetInt(EnabledKey, 1);
            return PlayerPrefs.GetInt(EnabledKey, 1) != 0;
        }
    }

    public static float Intensity
    {
        get { return Mathf.Clamp(PlayerPrefs.GetFloat(IntensityKey, 1f), 0.35f, 1.35f); }
    }

    public static void SetIntensity(float value, bool playPreview = true)
    {
        PlayerPrefs.SetFloat(IntensityKey, Mathf.Clamp(value, 0.35f, 1.35f));
        PlayerPrefs.Save();
        if (playPreview && Enabled) Play(Cue.Confirm, ConfirmPattern, 0.82f, true);
        Action handler = SettingChanged;
        if (handler != null) handler();
    }

    public static bool ControllerConnected
    {
        get { return HGHapticsDriver.ControllerConnected; }
    }

    public static OutputTarget CurrentOutput
    {
        get
        {
            if (!Enabled)
                return OutputTarget.Off;
            if (!ControllerConnected)
                return OutputTarget.Phone;
            return HGHapticsDriver.ControllerHasRumble
                ? OutputTarget.Controller
                : OutputTarget.ControllerWithoutRumble;
        }
    }

    public static void SetEnabled(bool enabled, bool playConfirmation = true)
    {
        PlayerPrefs.SetInt(EnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled)
            HGHapticsAndroidBackend.CancelAll();
        else if (playConfirmation)
            Play(Cue.Confirm, ConfirmPattern, 1f, true);

        Action handler = SettingChanged;
        if (handler != null)
            handler();
    }

    public static void ControlPressed(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.A:
            case KeyCode.D:
            case KeyCode.W:
            case KeyCode.S:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
                Play(Cue.Direction, DirectionPattern);
                break;

            case KeyCode.Space:
                Play(Cue.Jump, JumpPattern);
                break;

            case KeyCode.Mouse0:
                Play(Cue.AttackPress, AttackPressPattern);
                break;

            case KeyCode.T:
                Play(Cue.Throw, ThrowPattern);
                break;

            default:
                Play(Cue.UiTick, UiTickPattern);
                break;
        }
    }

    public static void ThrowCharge(float power)
    {
        int step = Mathf.Clamp(
            Mathf.FloorToInt(Mathf.Clamp01(power) * 4f) + 1,
            1, 4);

        if (step <= lastThrowChargeStep)
            return;

        lastThrowChargeStep = step;
        Play(
            Cue.ThrowCharge,
            ThrowChargePattern,
            Mathf.Lerp(0.55f, 1.18f, Mathf.Clamp01(power)),
            true);
    }

    public static void ResetThrowCharge()
    {
        lastThrowChargeStep = 0;
    }

    public static void ThrowRelease(float power)
    {
        power = Mathf.Clamp01(power);
        lastThrowChargeStep = 0;
        Play(
            Cue.ThrowRelease,
            ThrowReleasePattern,
            Mathf.Lerp(0.68f, 1.28f, power),
            true);
    }

    public static void UiTick()
    {
        Play(Cue.UiTick, UiTickPattern);
    }

    public static void Confirm()
    {
        Play(Cue.Confirm, ConfirmPattern, 1f, true);
    }

    public static void MeleeHit(float damage, float knockBack, bool unarmed)
    {
        float strength = Mathf.Clamp(
            0.72f + damage * 0.012f + knockBack * 0.008f + (unarmed ? -0.08f : 0f),
            0.65f, 1.25f);
        Play(Cue.MeleeHit, MeleePattern, strength);
    }

    public static void Gunshot(float knockBack, int shotsPerFire, int ammoType)
    {
        float strength = Mathf.Clamp(
            0.82f + knockBack * 0.015f + Mathf.Max(0, shotsPerFire - 1) * 0.04f,
            0.78f, 1.30f);

        if (ammoType == 2)
            Play(Cue.GunShotgun, ShotgunPattern, strength);
        else if (ammoType == 1)
            Play(Cue.GunRifle, RiflePattern, strength);
        else
            Play(Cue.GunPistol, PistolPattern, strength);
    }

    public static void Damage(float severity)
    {
        if (severity >= 22f)
            Play(Cue.DamageHeavy, DamageHeavyPattern, Mathf.Clamp(severity / 32f, 0.85f, 1.25f));
        else if (severity >= 8f)
            Play(Cue.DamageMedium, DamageMediumPattern, Mathf.Clamp(severity / 18f, 0.80f, 1.15f));
        else if (severity >= 1.2f)
            Play(Cue.DamageLight, DamageLightPattern, Mathf.Clamp(severity / 7f, 0.65f, 1.0f));
    }

    public static void Landing(float downwardSpeed)
    {
        if (downwardSpeed >= 21f)
            Play(Cue.LandingHeavy, LandingHeavyPattern, Mathf.Clamp(downwardSpeed / 31f, 0.82f, 1.25f));
        else if (downwardSpeed >= 9f)
            Play(Cue.LandingLight, LandingLightPattern, Mathf.Clamp(downwardSpeed / 18f, 0.65f, 1.0f));
    }

    public static void JumpPad()
    {
        Play(Cue.JumpPad, JumpPadPattern, 1f);
    }

    public static void CameraImpact(float amount)
    {
        if (amount >= 350f)
            Play(Cue.Explosion, ExplosionPattern, Mathf.Clamp(amount / 900f, 0.88f, 1.30f));
        else if (amount >= 45f)
            Play(Cue.Impact, ImpactPattern, Mathf.Clamp(amount / 110f, 0.65f, 1.15f));
    }

    public static void CriticalHeartbeat()
    {
        Play(Cue.Heartbeat, HeartbeatPattern);
    }

    public static void Earthquake(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        if (intensity < 0.035f)
            return;

        if (intensity >= 0.72f)
            Play(Cue.Earthquake, EarthquakeHeavyPattern, Mathf.Lerp(0.78f, 1.18f, intensity));
        else if (intensity >= 0.32f)
            Play(Cue.Earthquake, EarthquakeMediumPattern, Mathf.Lerp(0.70f, 1.08f, intensity));
        else
            Play(Cue.Earthquake, EarthquakeLowPattern, Mathf.Lerp(0.48f, 0.88f, intensity));
    }

    public static void EarthquakeDebris(float intensity)
    {
        if (intensity < 0.18f)
            return;
        Play(Cue.EarthquakeDebris, EarthquakeDebrisPattern,
            Mathf.Lerp(0.62f, 1.16f, Mathf.Clamp01(intensity)));
    }

    public static void RadiationLine(float proximity, bool exposed)
    {
        proximity = Mathf.Clamp01(proximity);
        if (proximity < 0.06f && !exposed)
            return;

        float now = Time.unscaledTime;
        if (now < nextRadiationTick)
            return;

        float interval = Mathf.Lerp(1.20f, exposed ? 0.12f : 0.22f, proximity);
        interval *= UnityEngine.Random.Range(0.72f, 1.28f);
        nextRadiationTick = now + Mathf.Max(0.08f, interval);

        Pattern pattern = exposed ? RadiationExposedPattern : RadiationPattern;
        float strength = Mathf.Lerp(exposed ? 0.78f : 0.52f, 1.22f, proximity);
        Play(Cue.Radiation, pattern, strength);
    }

    public static void GeyserWarning(float distance)
    {
        float strength = DistanceStrength(distance, 64f, 0.42f);
        if (strength > 0f)
            Play(Cue.GeyserWarning, GeyserWarningPattern, strength);
    }

    public static void GeyserBurst(float distance)
    {
        float strength = DistanceStrength(distance, 72f, 0.45f);
        if (strength > 0f)
            Play(Cue.GeyserBurst, GeyserBurstPattern, strength, true);
    }

    public static void GeyserFlow(float distance)
    {
        float strength = DistanceStrength(distance, 58f, 0.35f);
        if (strength <= 0f || Time.unscaledTime < nextGeyserFlowTick)
            return;

        nextGeyserFlowTick = Time.unscaledTime + 0.22f;
        Play(Cue.GeyserFlow, GeyserFlowPattern, strength);
    }

    public static void MineArmed(float distance)
    {
        float strength = DistanceStrength(distance, 50f, 0.55f);
        if (strength > 0f)
            Play(Cue.MineArmed, MineArmedPattern, strength, true);
    }

    public static void SoundCannonCharge(float progress, float distance)
    {
        progress = Mathf.Clamp01(progress);
        float distanceStrength = DistanceStrength(distance, 64f, 0.30f);
        if (distanceStrength <= 0f || Time.unscaledTime < nextSoundCannonPulse)
            return;

        float interval = Mathf.Lerp(0.72f, 0.12f, progress);
        nextSoundCannonPulse = Time.unscaledTime + interval;
        Play(Cue.SoundCannonCharge, SoundCannonChargePattern,
            distanceStrength * Mathf.Lerp(0.52f, 1.18f, progress));
    }

    public static void SoundCannonBlast(bool underwater, bool blocked = false)
    {
        float strength = blocked ? 0.64f : (underwater ? 1.30f : 1.08f);
        Play(Cue.SoundCannonBlast, SoundCannonBlastPattern, strength, true);
    }

    public static void GunSafety()
    {
        Play(Cue.GunSafety, GunSafetyPattern);
    }

    public static void GunLoad(bool magazine)
    {
        Play(Cue.GunLoad, GunLoadPattern, magazine ? 1.08f : 0.84f);
    }

    public static void GunUnload()
    {
        Play(Cue.GunUnload, GunUnloadPattern);
    }

    public static void GunTrigger(bool dry)
    {
        Play(Cue.GunTrigger, dry ? GunDryTriggerPattern : GunTriggerPattern,
            dry ? 1f : 0.72f);
    }

    public static void GunRack(bool pullingBack)
    {
        Play(Cue.GunRack, GunRackPattern, pullingBack ? 1.04f : 0.88f);
    }

    public static void GunJam()
    {
        Play(Cue.GunJam, GunJamPattern, 1f, true);
    }

    public static void LockpickResistance(float stuckTime)
    {
        if (Time.unscaledTime < nextLockpickPulse)
            return;

        float amount = Mathf.Clamp01(stuckTime / 0.5f);
        nextLockpickPulse = Time.unscaledTime + Mathf.Lerp(0.18f, 0.075f, amount);
        Play(Cue.LockpickResistance, LockpickResistancePattern,
            Mathf.Lerp(0.48f, 1.06f, amount));
    }

    public static void LockpickBreak()
    {
        Play(Cue.LockpickBreak, LockpickBreakPattern, 1f, true);
    }

    public static void LockpickSuccess()
    {
        Play(Cue.LockpickSuccess, LockpickSuccessPattern, 1f, true);
    }

    public static void DislocationHit(float speed, bool wrench)
    {
        float strength = Mathf.Clamp(speed / 22f, 0.62f, 1.22f);
        if (wrench)
            strength *= 0.86f;
        Play(Cue.DislocationHit, DislocationHitPattern, strength, true);
    }

    public static void DislocationSuccess()
    {
        Play(Cue.DislocationSuccess, DislocationSuccessPattern, 1f, true);
    }

    public static void AmputationCut(float speed, float progress)
    {
        if (speed < 1.2f || Time.unscaledTime < nextAmputationPulse)
            return;

        nextAmputationPulse = Time.unscaledTime + 0.085f;
        float strength = Mathf.Clamp(
            0.40f + speed * 0.035f + Mathf.Clamp01(progress) * 0.24f,
            0.42f, 1.02f);
        Play(Cue.AmputationCut, AmputationCutPattern, strength);
    }

    public static void AmputationComplete()
    {
        Play(Cue.AmputationComplete, AmputationCompletePattern, 1f, true);
    }

    public static void WorldExplosion(Vector2 position, float radius, float velocity)
    {
        Body local = null;
        try { if (PlayerCamera.main != null) local = PlayerCamera.main.body; } catch { }
        if (local == null) return;

        float now = Time.unscaledTime;
        if (now - lastExplosionAt < 0.12f && Vector2.Distance(position, lastExplosionPosition) < 1.0f)
            return;
        lastExplosionAt = now;
        lastExplosionPosition = position;
        suppressDeathUntil = Mathf.Max(suppressDeathUntil, now + WorldExplosionPattern.DurationSeconds + 0.10f);

        float distance = Vector2.Distance(position, local.transform.position);
        float effectiveRange = Mathf.Max(8f, Mathf.Max(radius * 3.2f, 16f));
        float falloff = 1f - Mathf.Clamp01(distance / effectiveRange);
        if (falloff <= 0f) return;

        
        
        float energy = Mathf.Clamp01(
            Mathf.Sqrt(Mathf.Max(0.1f, radius) / 12f) * 0.58f +
            Mathf.Sqrt(Mathf.Max(0f, velocity) / 65f) * 0.42f);
        float strength = Mathf.Lerp(0.30f, 1.30f, Mathf.Pow(falloff, 0.72f)) * Mathf.Lerp(0.66f, 1.18f, energy);

        try
        {
            if (Physics2D.Linecast(position, local.transform.position, LayerMask.GetMask("Ground")))
                strength *= 0.68f;
            if (local.inWater) strength *= 1.12f;
        }
        catch { }

        Play(Cue.WorldExplosion, WorldExplosionPattern, Mathf.Clamp(strength, 0.30f, 1.35f), true);
    }

    public static void FallingHazardStart(Vector2 position, float mass)
    {
        float distance = DistanceFromLocalPlayer(position);
        if (float.IsInfinity(distance)) return;
        float distanceStrength = DistanceStrength(distance, 48f, 0.22f);
        if (distanceStrength <= 0f) return;
        float massStrength = Mathf.Lerp(0.52f, 1.24f, Mathf.Clamp01(Mathf.Sqrt(Mathf.Max(1f, mass) / 60f)));
        Play(Cue.FallingHazardStart, FallingHazardStartPattern, distanceStrength * massStrength);
    }

    public static void FallingHazardImpact(Vector2 position, float speed, float mass, bool hitPlayer)
    {
        if (speed < 4f) return;
        float distance = DistanceFromLocalPlayer(position);
        if (float.IsInfinity(distance)) return;
        float distanceStrength = DistanceStrength(distance, 54f, 0.24f);
        if (distanceStrength <= 0f) return;
        float impulse = Mathf.Clamp01((speed * Mathf.Sqrt(Mathf.Max(1f, mass))) / 150f);
        float strength = distanceStrength * Mathf.Lerp(0.48f, 1.28f, impulse) * (hitPlayer ? 1.16f : 1f);
        Play(Cue.FallingHazardImpact, FallingHazardImpactPattern, Mathf.Clamp(strength, 0.32f, 1.35f), hitPlayer);
    }

    public static void MinigameOpened(string typeName)
    {
        Play(Cue.MinigameOpen, MinigameOpenPattern, 0.70f);
    }

    public static void MinigameClosed(string typeName)
    {
        Play(Cue.MinigameClose, MinigameClosePattern, 0.72f);
    }

    public static void MinigameGrab(float strength = 1f)
    {
        Play(Cue.MinigameGrab, MinigameGrabPattern, Mathf.Clamp(strength, 0.55f, 1.15f));
    }

    public static void KeypadPress(bool clear)
    {
        Play(Cue.KeypadPress, KeypadPressPattern, clear ? 0.78f : 1f, true);
    }

    public static void BandageWrap(bool completedWrap, float handSpeed)
    {
        float strength = Mathf.Clamp(0.52f + handSpeed * 0.018f + (completedWrap ? 0.24f : 0f), 0.48f, 1.08f);
        Play(Cue.BandageWrap, BandageWrapPattern, strength, completedWrap);
    }

    public static void ShrapnelPull(float speed, bool tweezers)
    {
        if (speed < 0.18f || Time.unscaledTime < nextShrapnelPulse) return;
        nextShrapnelPulse = Time.unscaledTime + Mathf.Lerp(0.16f, 0.055f, Mathf.Clamp01(speed / 8f));
        float strength = Mathf.Clamp(0.42f + speed * 0.055f, 0.42f, 1.12f) * (tweezers ? 0.78f : 1f);
        Play(Cue.ShrapnelPull, ShrapnelPullPattern, strength);
    }

    public static void ShrapnelSlip()
    {
        Play(Cue.ShrapnelSlip, ShrapnelSlipPattern, 1f, true);
    }

    public static void SyringePuncture()
    {
        Play(Cue.SyringePuncture, SyringePuncturePattern, 1f, true);
    }

    public static void SyringeInject(float depth, float speed)
    {
        if (Time.unscaledTime < nextSyringePulse) return;
        depth = Mathf.Clamp01(depth);
        nextSyringePulse = Time.unscaledTime + Mathf.Lerp(0.16f, 0.055f, depth);
        Play(Cue.SyringeInject, SyringeInjectPattern,
            Mathf.Clamp(0.35f + depth * 0.42f + Mathf.Abs(speed) * 0.012f, 0.35f, 0.92f));
    }

    public static void SyringeSlip()
    {
        Play(Cue.SyringeSlip, SyringeSlipPattern, 1f, true);
    }

    public static void SelfHarmTap(float progress)
    {
        progress = Mathf.Clamp01(progress);
        Play(Cue.SelfHarmTap, SelfHarmTapPattern, Mathf.Lerp(0.52f, 1.14f, progress), true);
    }

    public static void SelfHarmCut(float progress)
    {
        if (Time.unscaledTime < nextSelfHarmCutPulse) return;
        nextSelfHarmCutPulse = Time.unscaledTime + Mathf.Lerp(0.12f, 0.055f, Mathf.Clamp01(progress));
        Play(Cue.SelfHarmCut, SelfHarmCutPattern, Mathf.Lerp(0.70f, 1.15f, Mathf.Clamp01(progress)));
    }

    public static void AEDPadPlaced(bool finalPad)
    {
        Play(Cue.AEDPad, AEDPadPattern, finalPad ? 1.12f : 0.86f, true);
    }

    public static void AEDShock(bool success = true)
    {
        Play(Cue.AEDShock, AEDShockPattern, success ? 1.12f : 0.88f, true);
    }

    public static void DefibDial(float deltaDegrees)
    {
        if (Mathf.Abs(deltaDegrees) < 1.5f || Time.unscaledTime < nextDefibDialPulse) return;
        nextDefibDialPulse = Time.unscaledTime + 0.045f;
        Play(Cue.DefibDial, DefibDialPattern, Mathf.Clamp(0.42f + Mathf.Abs(deltaDegrees) * 0.016f, 0.42f, 0.85f));
    }

    public static void DefibCharge()
    {
        Play(Cue.DefibCharge, DefibChargePattern, 1f, true);
    }

    public static void DefibShock(bool success, float charge)
    {
        float strength = Mathf.Clamp(0.82f + Mathf.Clamp01(charge) * 0.35f, 0.82f, 1.22f);
        if (!success) strength *= 0.88f;
        Play(Cue.DefibShock, DefibShockPattern, strength, true);
    }

    public static void HandCrank(float deltaDegrees)
    {
        float d = Mathf.Abs(deltaDegrees);
        if (d < 1.2f || Time.unscaledTime < nextHandCrankPulse) return;
        nextHandCrankPulse = Time.unscaledTime + Mathf.Lerp(0.075f, 0.028f, Mathf.Clamp01(d / 24f));
        Play(Cue.HandCrank, HandCrankPattern, Mathf.Clamp(0.40f + d * 0.022f, 0.40f, 0.92f));
    }

    public static void SelfDestructPulse(float progress)
    {
        progress = Mathf.Clamp01(progress);
        Play(Cue.SelfDestructPulse, SelfDestructPulsePattern,
            Mathf.Lerp(0.58f, 1.28f, progress * progress), true);
    }

    private static float DistanceFromLocalPlayer(Vector2 position)
    {
        try
        {
            if (PlayerCamera.main != null && PlayerCamera.main.body != null)
                return Vector2.Distance(position, PlayerCamera.main.body.transform.position);
        }
        catch { }
        return float.PositiveInfinity;
    }

    private static float DistanceStrength(float distance, float maxDistance, float minimum)
    {
        float normalized = 1f - Mathf.Clamp01(distance / Mathf.Max(1f, maxDistance));
        if (normalized <= 0f)
            return 0f;
        return Mathf.Lerp(minimum, 1.15f, normalized);
    }

    public static void Death()
    {
        
        
        if (Time.unscaledTime < suppressDeathUntil) return;
        Play(Cue.Death, DeathPattern, 1f, true);
    }

    private static void Play(
        Cue cue,
        Pattern pattern,
        float strength = 1f,
        bool force = false)
    {
        if (!Enabled || !Application.isFocused)
            return;

        HGHapticsDriver.EnsureReady();

        float now = Time.unscaledTime;
        float last;
        if (!force && LastPlayed.TryGetValue(cue, out last) &&
            now - last < pattern.cooldown)
        {
            return;
        }

        if (!force && now < protectedUntil && pattern.priority < protectedPriority)
            return;

        LastPlayed[cue] = now;
        protectedPriority = pattern.priority;
        protectedUntil = now + Mathf.Clamp(
            pattern.DurationSeconds * 0.72f,
            0.035f,
            0.65f);

        HGHapticsAndroidBackend.Play(
            pattern,
            Mathf.Clamp(strength * Intensity, 0.20f, 1.35f),
            ControllerConnected);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        HGHapticsDriver.EnsureReady();
    }
}

public sealed class HGHapticsDriver : MonoBehaviour
{
    private static HGHapticsDriver instance;
    private static bool controllerConnected; 
    private static bool controllerHasRumble;

    private float nextControllerScan;

    public static bool ControllerConnected
    {
        get { return controllerConnected; }
    }

    public static bool ControllerHasRumble
    {
        get { return controllerHasRumble; }
    }

    public static void EnsureReady()
    {
        if (instance != null)
            return;

        GameObject go = new GameObject("HG Haptics Driver");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<HGHapticsDriver>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        HGHapticsAndroidBackend.Initialize();
        RefreshController(true);
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextControllerScan)
        {
            nextControllerScan = Time.unscaledTime + 0.75f;
            RefreshController(false);
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            HGHapticsAndroidBackend.CancelAll();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            HGHapticsAndroidBackend.CancelAll();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            HGHapticsAndroidBackend.CancelAll();
            instance = null;
        }
    }

    private static void RefreshController(bool force)
    {
        bool oldConnected = controllerConnected;
        bool oldRumble = controllerHasRumble;

        controllerConnected = HasUnityJoystickName();
        bool nativeConnected;
        bool nativeRumble;
        HGHapticsAndroidBackend.RefreshControllerDevices(
            out nativeConnected,
            out nativeRumble);

        controllerConnected |= nativeConnected;
        controllerHasRumble = nativeRumble;

        if (!force &&
            (oldConnected != controllerConnected || oldRumble != controllerHasRumble))
        {
            
            HGHapticsAndroidBackend.CancelAll();
        }
    }

    private static bool HasUnityJoystickName()
    {
        string[] names = Input.GetJoystickNames();
        if (names == null)
            return false;

        for (int i = 0; i < names.Length; i++)
        {
            if (!string.IsNullOrEmpty(names[i]) &&
                !string.IsNullOrEmpty(names[i].Trim()))
            {
                return true;
            }
        }

        return false;
    }
}

internal static class HGHapticsAndroidBackend
{
    private const int SourceGamepad = 0x00000401;
    private const int SourceJoystick = 0x01000010;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject phoneVibrator;
    private static int sdkInt;
#endif
    private static readonly List<AndroidJavaObject> ControllerVibrators =
        new List<AndroidJavaObject>();
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass version =
                new AndroidJavaClass("android.os.Build$VERSION"))
            {
                sdkInt = version.GetStatic<int>("SDK_INT");
            }

            using (AndroidJavaClass unityPlayer =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity =
                    unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                if (sdkInt >= 31)
                {
                    using (AndroidJavaObject manager =
                        activity.Call<AndroidJavaObject>(
                            "getSystemService", "vibrator_manager"))
                    {
                        if (manager != null)
                            phoneVibrator = manager.Call<AndroidJavaObject>(
                                "getDefaultVibrator");
                    }
                }
                else
                {
                    phoneVibrator = activity.Call<AndroidJavaObject>(
                        "getSystemService", "vibrator");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Haptics initialization failed: " + ex.Message);
        }
#endif
    }

    public static void RefreshControllerDevices(
        out bool controllerConnected,
        out bool controllerHasRumble)
    {
        controllerConnected = false;
        controllerHasRumble = false;

#if UNITY_ANDROID && !UNITY_EDITOR
        Initialize();
        DisposeControllerVibrators();

        try
        {
            using (AndroidJavaClass inputDeviceClass =
                new AndroidJavaClass("android.view.InputDevice"))
            {
                int[] deviceIds =
                    inputDeviceClass.CallStatic<int[]>("getDeviceIds");
                if (deviceIds == null)
                    return;

                for (int i = 0; i < deviceIds.Length; i++)
                {
                    using (AndroidJavaObject device =
                        inputDeviceClass.CallStatic<AndroidJavaObject>(
                            "getDevice", deviceIds[i]))
                    {
                        if (device == null)
                            continue;

                        bool gamepad = false;
                        try
                        {
                            gamepad =
                                device.Call<bool>("supportsSource", SourceGamepad) ||
                                device.Call<bool>("supportsSource", SourceJoystick);
                        }
                        catch
                        {
                            int sources = device.Call<int>("getSources");
                            gamepad =
                                (sources & SourceGamepad) == SourceGamepad ||
                                (sources & SourceJoystick) == SourceJoystick;
                        }

                        if (!gamepad)
                            continue;

                        bool isVirtual = false;
                        try { isVirtual = device.Call<bool>("isVirtual"); }
                        catch { }
                        if (isVirtual)
                            continue;

                        controllerConnected = true;
                        AddControllerVibrators(device);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Controller haptics scan failed: " + ex.Message);
        }

        controllerHasRumble = ControllerVibrators.Count > 0;
#endif
    }

    private static void AddControllerVibrators(AndroidJavaObject device)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (sdkInt >= 31)
            {
                using (AndroidJavaObject manager =
                    device.Call<AndroidJavaObject>("getVibratorManager"))
                {
                    if (manager == null)
                        return;

                    int[] ids = manager.Call<int[]>("getVibratorIds");
                    if (ids == null)
                        return;

                    for (int i = 0; i < ids.Length && ControllerVibrators.Count < 2; i++)
                    {
                        AndroidJavaObject vibrator =
                            manager.Call<AndroidJavaObject>("getVibrator", ids[i]);
                        if (HasVibrator(vibrator))
                            ControllerVibrators.Add(vibrator);
                        else if (vibrator != null)
                            vibrator.Dispose();
                    }
                }
            }
            else
            {
                AndroidJavaObject vibrator =
                    device.Call<AndroidJavaObject>("getVibrator");
                if (HasVibrator(vibrator))
                    ControllerVibrators.Add(vibrator);
                else if (vibrator != null)
                    vibrator.Dispose();
            }
        }
        catch
        {
            
        }
#endif
    }

    private static bool HasVibrator(AndroidJavaObject vibrator)
    {
        if (vibrator == null)
            return false;

        try { return vibrator.Call<bool>("hasVibrator"); }
        catch { return false; }
    }

    public static void Play(
        HGHaptics.Pattern pattern,
        float strength,
        bool controllerConnected)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Initialize();

        if (controllerConnected)
        {
            
            
            Cancel(phoneVibrator);

            if (ControllerVibrators.Count == 0)
                return;

            if (ControllerVibrators.Count == 1)
            {
                Vibrate(
                    ControllerVibrators[0],
                    pattern.timings,
                    pattern.amplitudes,
                    strength * pattern.controllerRightScale);
            }
            else
            {
                Vibrate(
                    ControllerVibrators[0],
                    pattern.timings,
                    pattern.amplitudes,
                    strength * pattern.controllerLeftScale);
                Vibrate(
                    ControllerVibrators[1],
                    pattern.timings,
                    pattern.amplitudes,
                    strength * pattern.controllerRightScale);
            }

            return;
        }

        if (phoneVibrator != null)
        {
            Vibrate(
                phoneVibrator,
                pattern.timings,
                pattern.amplitudes,
                strength);
        }
        else
        {
            
            
            Handheld.Vibrate();
        }
#endif
    }

    private static void Vibrate(
        AndroidJavaObject vibrator,
        long[] timings,
        int[] amplitudes,
        float scale)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!HasVibrator(vibrator))
            return;

        try
        {
            if (sdkInt >= 26)
            {
                bool amplitudeControl = false;
                try { amplitudeControl = vibrator.Call<bool>("hasAmplitudeControl"); }
                catch { }

                using (AndroidJavaClass effectClass =
                    new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    AndroidJavaObject effect;
                    if (amplitudeControl)
                    {
                        int[] scaled = ScaleAmplitudes(amplitudes, scale);
                        effect = effectClass.CallStatic<AndroidJavaObject>(
                            "createWaveform", timings, scaled, -1);
                    }
                    else
                    {
                        effect = effectClass.CallStatic<AndroidJavaObject>(
                            "createWaveform", timings, -1);
                    }

                    using (effect)
                    {
                        vibrator.Call("vibrate", effect);
                    }
                }
            }
            else
            {
                vibrator.Call("vibrate", timings, -1);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Could not play haptic waveform: " + ex.Message);
        }
#endif
    }

    private static int[] ScaleAmplitudes(int[] source, float scale)
    {
        int[] result = new int[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] <= 0)
                result[i] = 0;
            else
                result[i] = Mathf.Clamp(
                    Mathf.RoundToInt(source[i] * scale), 1, 255);
        }
        return result;
    }

    public static void CancelAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Cancel(phoneVibrator);
        for (int i = 0; i < ControllerVibrators.Count; i++)
            Cancel(ControllerVibrators[i]);
#endif
    }

    private static void Cancel(AndroidJavaObject vibrator)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (vibrator == null)
            return;
        try { vibrator.Call("cancel"); }
        catch { }
#endif
    }

    private static void DisposeControllerVibrators()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        for (int i = 0; i < ControllerVibrators.Count; i++)
        {
            if (ControllerVibrators[i] != null)
                ControllerVibrators[i].Dispose();
        }
        ControllerVibrators.Clear();
#endif
    }
}
