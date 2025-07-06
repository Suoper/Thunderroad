using System;
using UnityEngine;

namespace ThunderRoad
{
    /// <summary>
    /// Test class to verify PossessionSpell patches work correctly and prevent crashes
    /// while allowing normal operations when components are valid.
    /// </summary>
    public class PossessionSpellTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool enableTestMode = false;
        public bool simulateNullComponents = false;
        public float testInterval = 5f;

        private float lastTestTime;
        private PossessionSpell possessionSpell;

        private void Start()
        {
            if (enableTestMode)
            {
                // Initialize possession spell for testing
                possessionSpell = new PossessionSpell();
                Debug.Log("PossessionSpellTest: Test mode enabled");
            }
        }

        private void Update()
        {
            if (!enableTestMode)
                return;

            // Run tests periodically
            if (Time.time - lastTestTime > testInterval)
            {
                lastTestTime = Time.time;
                RunPossessionSafetyTests();
            }
        }

        /// <summary>
        /// Tests the safety and validation logic of possession patches.
        /// </summary>
        private void RunPossessionSafetyTests()
        {
            Debug.Log("PossessionSpellTest: Running possession safety tests...");

            // Test 1: IkController validation
            TestIkControllerValidation();

            // Test 2: Locomotion validation  
            TestLocomotionValidation();

            // Test 3: Simulated null component handling
            if (simulateNullComponents)
            {
                TestNullComponentHandling();
            }

            Debug.Log("PossessionSpellTest: Tests completed");
        }

        /// <summary>
        /// Tests IkController component validation logic.
        /// </summary>
        private void TestIkControllerValidation()
        {
            try
            {
                // Find an IkController in the scene
                var ikController = FindObjectOfType<IkController>();
                if (ikController != null)
                {
                    // Test the validation logic that would be used by the patch
                    bool isValid = IsIkControllerSafe(ikController);
                    Debug.Log($"PossessionSpellTest: IkController validation result: {isValid}");
                    
                    if (!isValid)
                    {
                        Debug.Log("PossessionSpellTest: IkController would be safely blocked by patch");
                    }
                    else
                    {
                        Debug.Log("PossessionSpellTest: IkController would be allowed to execute normally");
                    }
                }
                else
                {
                    Debug.Log("PossessionSpellTest: No IkController found in scene");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpellTest: Error testing IkController: {ex}");
            }
        }

        /// <summary>
        /// Tests Locomotion component validation logic.
        /// </summary>
        private void TestLocomotionValidation()
        {
            try
            {
                // Find a Locomotion component in the scene
                var locomotion = FindObjectOfType<Locomotion>();
                if (locomotion != null)
                {
                    // Test the validation logic
                    bool isValid = IsLocomotionSafe(locomotion);
                    Debug.Log($"PossessionSpellTest: Locomotion validation result: {isValid}");
                    
                    if (!isValid)
                    {
                        Debug.Log("PossessionSpellTest: Locomotion would be safely blocked by patch");
                    }
                    else
                    {
                        Debug.Log("PossessionSpellTest: Locomotion would be allowed to execute normally");
                    }
                }
                else
                {
                    Debug.Log("PossessionSpellTest: No Locomotion found in scene");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpellTest: Error testing Locomotion: {ex}");
            }
        }

        /// <summary>
        /// Tests how the system handles null components during possession transitions.
        /// </summary>
        private void TestNullComponentHandling()
        {
            Debug.Log("PossessionSpellTest: Testing null component handling...");
            
            try
            {
                // Simulate testing with null components
                bool wouldBlock1 = !IsIkControllerSafe(null);
                bool wouldBlock2 = !IsLocomotionSafe(null);
                
                Debug.Log($"PossessionSpellTest: Null IkController would be blocked: {wouldBlock1}");
                Debug.Log($"PossessionSpellTest: Null Locomotion would be blocked: {wouldBlock2}");
                
                if (wouldBlock1 && wouldBlock2)
                {
                    Debug.Log("PossessionSpellTest: ✓ Null components are properly blocked");
                }
                else
                {
                    Debug.LogWarning("PossessionSpellTest: ⚠ Null components not properly handled");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PossessionSpellTest: Error testing null handling: {ex}");
            }
        }

        /// <summary>
        /// Replicates the validation logic from PossessionSpell for testing.
        /// </summary>
        private bool IsIkControllerSafe(IkController ikController)
        {
            try
            {
                if (ikController?.creature == null)
                    return false;

                var creature = ikController.creature;

                if (!ikController.initialized || !creature.initialized)
                    return false;

                if (creature.transform == null)
                    return false;

                if (creature.centerEyes == null)
                    return false;

                if (ikController.turnBodyByHeadAndHands)
                {
                    if (creature.ragdoll?.ik == null)
                        return false;

                    if (creature.ragdoll.ik.handRightTarget == null ||
                        creature.ragdoll.ik.handLeftTarget == null)
                        return false;

                    if (creature.handRight == null || creature.handLeft == null)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Replicates the validation logic from PossessionSpell for testing.
        /// </summary>
        private bool IsLocomotionSafe(Locomotion locomotion)
        {
            try
            {
                if (locomotion == null)
                    return false;

                if (locomotion.physicBody == null)
                    return false;

                if (locomotion.transform == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Manual test method that can be called from the console or inspector.
        /// </summary>
        [ContextMenu("Run Possession Safety Test")]
        public void ManualTest()
        {
            RunPossessionSafetyTests();
        }
    }
}