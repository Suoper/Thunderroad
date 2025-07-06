# PossessionSpell - Bulletproof Harmony Patches

## Overview
The PossessionSpell provides Harmony patches that fix overly aggressive blocking in the possession system while maintaining crash protection during possession transitions.

## Problem Solved
- **NullReferenceExceptions** in `ThunderRoad.IkController.ManagedUpdate` at `UpdateBodyRotation()` lines 91, 93-94, 97-98
- **NullReferenceExceptions** in `ThunderRoad.PlayerControl.UpdateRunning()` at line 992  
- **Overly aggressive patches** blocking normal game operations when components are valid

## Solution Implementation

### 1. IkController.ManagedUpdate Patch
- Validates initialization state before allowing execution
- Checks creature and component readiness
- Only blocks during genuine possession risks

### 2. IkController.UpdateBodyRotation Patch  
Comprehensive safety checks for specific null reference sources:
- **Line 91**: `creature.centerEyes.forward.ToXZ()` - validates centerEyes
- **Lines 93-94**: `creature.ragdoll.ik.handRightTarget.position` - validates ragdoll, ik, and hand targets
- **Lines 97-98**: `creature.handRight.GetArmLenghtRatio()` - validates hand references

### 3. Locomotion.ManagedUpdate Patch
- Safety checks for physics body and running calculations
- Prevents crashes during locomotion state transitions

### 4. PlayerControl.UpdateRunning Patch
- Uses flexible reflection-based targeting to find method across assemblies
- Validates player hands and physics bodies before execution

## Key Features

### Intelligent Blocking
- ✅ **Selective**: Only blocks execution during genuine risks
- ✅ **Permissive**: Allows normal operations when components are valid  
- ✅ **Recovery**: Graceful handling of temporary component unavailability
- ✅ **Logging**: Detailed debug information for possession transitions

### Safety Improvements
- Validates creature initialization before any IK operations
- Checks transform, ragdoll, and hand target availability
- Validates physics body for locomotion calculations  
- Uses try-catch blocks to prevent patch failures from crashing the game

## Usage

1. **Automatic**: Patches are applied when the PossessionSpell is loaded
2. **Testing**: Use `PossessionSpellTest` component to verify patch behavior
3. **Manual Testing**: Call `PossessionSpellTest.ManualTest()` from console

## Testing
The `PossessionSpellTest` class provides:
- Periodic safety validation tests
- Null component handling verification  
- Manual test triggering
- Detailed logging of patch behavior

## Files
- `PossessionSpell.cs` - Main Harmony patches
- `PossessionSpellTest.cs` - Testing and validation
- Enhanced project references for Harmony support

## Result
- **Eliminates** NullReferenceExceptions in IkController and PlayerControl
- **Maintains** possession system stability and crash prevention
- **Allows** normal game operations to continue uninterrupted when safe
- **Preserves** bulletproof protection during actual possession transitions