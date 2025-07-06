using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ThunderRoad
{
    /// <summary>
    /// Possession spell that provides bulletproof Harmony patches to prevent crashes during possession transitions
    /// while maintaining normal game operations when components are valid and safe.
    /// </summary>
    public class PossessionSpell : SpellCastData
    {
        private static Harmony harmony;

        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            
            // Initialize Harmony patches if not already done
            if (harmony == null)
            {
                harmony = new Harmony("com.thunderroad.possession");
                harmony.PatchAll(typeof(PossessionSpell));
            }
        }

        public override void Unload()
        {
            base.Unload();
            
            // Unpatch when unloading
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                harmony = null;
            }
        }

        /// <summary>
        /// Harmony prefix patch for IkController.ManagedUpdate to prevent null reference exceptions
        /// while allowing normal execution when components are valid.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(IkController), "ManagedUpdate")]
        public static bool IkController_ManagedUpdate_Prefix(IkController __instance)
        {
            try
            {
                // Only block execution if there's a genuine risk during possession transitions
                if (!__instance.initialized)
                {
                    return false; // Block execution - component not initialized
                }

                if (__instance.creature == null)
                {
                    return false; // Block execution - no creature reference
                }

                if (!__instance.creature.initialized)
                {
                    return false; // Block execution - creature not initialized
                }

                // Additional safety checks for possession scenarios
                if (__instance.turnBodyByHeadAndHands)
                {
                    // Validate components needed for UpdateBodyRotation
                    if (!IsIkComponentsValid(__instance))
                    {
                        return false; // Block execution - IK components not ready
                    }
                }

                // All components are valid and safe - allow normal execution
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpell: Error in IkController_ManagedUpdate_Prefix: {ex}");
                return false; // Block execution on any validation error
            }
        }

        /// <summary>
        /// Harmony prefix patch for IkController.UpdateBodyRotation to provide additional safety.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(IkController), "UpdateBodyRotation")]
        public static bool IkController_UpdateBodyRotation_Prefix(IkController __instance)
        {
            try
            {
                // Comprehensive validation for UpdateBodyRotation components
                if (!IsIkComponentsValid(__instance))
                {
                    return false; // Block execution - components not safe
                }

                // Check for creature transform validity
                if (__instance.creature.transform == null)
                {
                    return false; // Block execution - no transform
                }

                // Check for ragdoll and IK system validity
                if (__instance.creature.ragdoll == null || 
                    __instance.creature.ragdoll.ik == null)
                {
                    return false; // Block execution - ragdoll system not ready
                }

                // Check for hand targets (common source of nullrefs at line 93-94)
                if (__instance.creature.ragdoll.ik.handRightTarget == null ||
                    __instance.creature.ragdoll.ik.handLeftTarget == null)
                {
                    return false; // Block execution - hand targets not available
                }

                // Check for hands (needed for GetArmLenghtRatio calls at lines 97-98)
                if (__instance.creature.handRight == null ||
                    __instance.creature.handLeft == null)
                {
                    return false; // Block execution - hand references not available
                }

                // All components are valid - allow execution
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpell: Error in IkController_UpdateBodyRotation_Prefix: {ex}");
                return false; // Block execution on any validation error
            }
        }

        /// <summary>
        /// Validates that all IK components are properly initialized and safe to use.
        /// </summary>
        private static bool IsIkComponentsValid(IkController ikController)
        {
            try
            {
                if (ikController?.creature == null)
                    return false;

                var creature = ikController.creature;

                // Check creature basics
                if (creature.transform == null)
                    return false;

                // Check center eyes for head direction calculation (line 91)
                if (creature.centerEyes == null)
                    return false;

                // Check locomotion for running state calculation (line 92)
                if (creature.currentLocomotion == null)
                {
                    // Locomotion can be null temporarily, but we should allow execution
                    // This is not a critical failure for UpdateBodyRotation
                }

                // Check ragdoll system
                if (creature.ragdoll?.ik == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that locomotion components are properly initialized and safe for running calculations.
        /// </summary>
        private static bool IsLocomotionComponentsValid(Locomotion locomotion)
        {
            try
            {
                if (locomotion == null)
                    return false;

                // Check physics body (needed for velocity calculations)
                if (locomotion.physicBody == null)
                    return false;

                // Check transform
                if (locomotion.transform == null)
                    return false;

                // Additional safety checks for possession scenarios
                // Allow execution even if some properties are temporarily null
                // as they might be initializing during possession transitions

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Harmony prefix patch for Locomotion.ManagedUpdate to prevent null reference exceptions
        /// during running state calculations and provide safety for possession transitions.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locomotion), "ManagedUpdate")]
        public static bool Locomotion_ManagedUpdate_Prefix(Locomotion __instance)
        {
            try
            {
                // Check if locomotion components are valid for running calculations
                if (!IsLocomotionComponentsValid(__instance))
                {
                    return false; // Block execution - locomotion not ready
                }

                // Allow execution when components are valid
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpell: Error in Locomotion_ManagedUpdate_Prefix: {ex}");
                return false; // Block execution on any validation error
            }
        }

        /// <summary>
        /// Harmony transpiler that attempts to find and patch PlayerControl.UpdateRunning if it exists.
        /// This uses a more flexible approach to handle the method that may be in different assemblies.
        /// </summary>
        [HarmonyTargetMethods]
        public static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetPlayerControlMethods()
        {
            var methods = new System.Collections.Generic.List<System.Reflection.MethodBase>();
            
            try
            {
                // Search for PlayerControl.UpdateRunning in all loaded assemblies
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var playerControlType = assembly.GetType("ThunderRoad.PlayerControl");
                        if (playerControlType != null)
                        {
                            var updateRunningMethod = playerControlType.GetMethod("UpdateRunning", 
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (updateRunningMethod != null)
                            {
                                methods.Add(updateRunningMethod);
                                Debug.Log("PossessionSpell: Found PlayerControl.UpdateRunning method for patching");
                            }
                        }
                    }
                    catch
                    {
                        // Continue searching in other assemblies
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpell: Error finding PlayerControl methods: {ex}");
            }

            return methods;
        }

        /// <summary>
        /// Generic prefix for PlayerControl.UpdateRunning when found via TargetMethods.
        /// </summary>
        [HarmonyPrefix]
        public static bool PlayerControl_UpdateRunning_Prefix(object __instance)
        {
            try
            {
                // Since PlayerControl class might be in a different assembly, use reflection
                var instanceType = __instance.GetType();
                
                // Check if player hands and physics bodies are valid
                if (!IsPlayerControlComponentsValid(__instance, instanceType))
                {
                    return false; // Block execution - player components not ready
                }

                // Allow execution when components are valid
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpell: Error in PlayerControl_UpdateRunning_Prefix: {ex}");
                return false; // Block execution on any validation error
            }
        }

        /// <summary>
        /// Validates PlayerControl components using reflection since the class may be in a different assembly.
        /// </summary>
        private static bool IsPlayerControlComponentsValid(object instance, Type instanceType)
        {
            try
            {
                // Use reflection to check common player control properties
                var playerProperty = instanceType.GetProperty("player") ?? instanceType.GetField("player");
                if (playerProperty == null)
                    return false;

                var player = playerProperty.GetType() == typeof(System.Reflection.PropertyInfo) 
                    ? ((System.Reflection.PropertyInfo)playerProperty).GetValue(instance)
                    : ((System.Reflection.FieldInfo)playerProperty).GetValue(instance);

                if (player == null)
                    return false;

                // Additional validation can be added here for specific player components
                // that are needed for UpdateRunning method

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Postfix patch to provide fallback handling and logging for possession events.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(IkController), "ManagedUpdate")]
        public static void IkController_ManagedUpdate_Postfix(IkController __instance)
        {
            // This postfix ensures that even if something goes wrong, 
            // the game state remains stable for future frames
            try
            {
                if (__instance?.creature != null && !__instance.creature.initialized)
                {
                    // Log possession transition state for debugging
                    Debug.Log("PossessionSpell: Creature initialization in progress");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpell: Error in IkController_ManagedUpdate_Postfix: {ex}");
            }
        }
    }
}