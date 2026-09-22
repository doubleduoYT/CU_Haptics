# Haptics call sites

아래 코드는 제공된 `CU_Mobile_SourceCode.zip`에서 실제로 확인된 `HGHaptics.*` 호출들이야. `HGHaptics` 클래스 구현 자체는 ZIP에 없어서 호출부만 정리했어.

## Assembly-CSharp/AmputationMinigame.cs
```csharp
HGHaptics.AmputationComplete();
HGHaptics.AmputationCut(Mathf.Abs(Minigame.game.handVelocity.x), cutProgress);
```

## Assembly-CSharp/BandageMinigame.cs
```csharp
HGHaptics.BandageWrap(didWrap, Minigame.game.handVelocity.magnitude);
```

## Assembly-CSharp/Body.cs
```csharp
HGHaptics.MeleeHit(atk.damage, atk.knockBack, atk.unarmed);
```

## Assembly-CSharp/DamagingCrate.cs
```csharp
HGHaptics.FallingHazardImpact(base.transform.position, prevFrameSpeed.magnitude, rb != null ? rb.mass : 10f, hgDirectHit);
```

## Assembly-CSharp/DislocationMinigame.cs
```csharp
HGHaptics.DislocationHit(boneVelocity.magnitude, hasWrench);
HGHaptics.DislocationSuccess();
```

## Assembly-CSharp/GeyserScript.cs
```csharp
HGHaptics.GeyserWarning(Vector2.Distance(base.transform.position, PlayerCamera.main.body.transform.position));
HGHaptics.GeyserBurst(Vector2.Distance(base.transform.position, PlayerCamera.main.body.transform.position));
HGHaptics.GeyserFlow(Vector2.Distance(base.transform.position, PlayerCamera.main.body.transform.position));
```

## Assembly-CSharp/GunScript.cs
```csharp
HGHaptics.GunSafety();
HGHaptics.GunLoad(false);
HGHaptics.GunLoad(true);
HGHaptics.GunUnload();
HGHaptics.GunTrigger(!hgWillFire);
HGHaptics.GunRack(false);
HGHaptics.GunJam();
HGHaptics.GunRack(true);
HGHaptics.Gunshot(knockBack, shotsPerFire, (int)ammoType);
```

## Assembly-CSharp/JumpPadScript.cs
```csharp
HGHaptics.JumpPad();
```

## Assembly-CSharp/KeypadMinigame.cs
```csharp
HGHaptics.KeypadPress(false);
HGHaptics.KeypadPress(true);
```

## Assembly-CSharp/LockpingMinigame.cs
```csharp
HGHaptics.LockpickSuccess();
HGHaptics.LockpickResistance(timeWasStuck);
HGHaptics.LockpickBreak();
```

## Assembly-CSharp/MineScript.cs
```csharp
HGHaptics.MineArmed(Vector2.Distance(base.transform.position, PlayerCamera.main.body.transform.position));
```

## Assembly-CSharp/MinigameBase.cs
```csharp
HGHaptics.MinigameOpened(currentMinigame.GetType().Name);
HGHaptics.MinigameClosed(currentMinigame.GetType().Name);
```

## Assembly-CSharp/RadiationLine.cs
```csharp
float hgDistance = Mathf.Abs(body.transform.position.y - base.transform.position.y);
HGHaptics.RadiationLine(
    Mathf.Clamp01(1f - hgDistance / 75f),
    body.transform.position.y > base.transform.position.y);
```

## Assembly-CSharp/SelfHarmMinigame.cs
```csharp
HGHaptics.SelfHarmCut(0f);
HGHaptics.SelfHarmCut(currentCut.fillAmount);
HGHaptics.SelfHarmTap((15f - (float)triesLeft) / 15f);
```

## Assembly-CSharp/Shaker.cs
```csharp
HGHaptics.CameraImpact(amount);
```

## Assembly-CSharp/ShrapnelMinigame.cs
```csharp
HGHaptics.ShrapnelSlip();
HGHaptics.MinigameGrab(hasTweezers ? 0.72f : 0.9f);
HGHaptics.ShrapnelPull(Mathf.Abs(Minigame.game.handVelocity.y), hasTweezers);
HGHaptics.Confirm();
```

## Assembly-CSharp/SoundCannon.cs
```csharp
HGHaptics.SoundCannonCharge(chargeTime / 5f, Vector2.Distance(base.transform.position, vector2));
HGHaptics.SoundCannonBlast(true);
HGHaptics.SoundCannonBlast(false);
HGHaptics.SoundCannonBlast(false, true);
```

## Assembly-CSharp/StalactiteDropper.cs
```csharp
HGHaptics.FallingHazardStart(base.transform.position, dropMass);
```

## Assembly-CSharp/SyringeMinigame.cs
```csharp
HGHaptics.MinigameGrab(0.72f);
HGHaptics.SyringeSlip();
HGHaptics.SyringePuncture();
HGHaptics.SyringeInject(num2, Mathf.Abs(Minigame.game.handVelocity.y));
```

## Assembly-CSharp/WorldGeneration.cs
```csharp
HGHaptics.Earthquake(earthquakeIntensity);
HGHaptics.EarthquakeDebris(earthquakeIntensity);
HGHaptics.WorldExplosion(PlayerCamera.main.body.transform.position + Vector3.down * 11f, 8f, 55f);
HGHaptics.WorldExplosion(param.position, param.range, param.velocity);
```

## BlueOK/BlueOKItem.cs
```csharp
HGHaptics.WorldExplosion(origin, BlastRadius, LaunchSpeed);
```
