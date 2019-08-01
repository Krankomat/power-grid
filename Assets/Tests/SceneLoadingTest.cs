using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement; 

namespace Tests
{
    public class SceneLoadingTest
    {

        [UnityTest]
        public IEnumerator TestSceneLoading()
        {
            
            // -------- SET UP -------- 

            //Store the test scene in a temp variable 
            Scene originalScene = SceneManager.GetActiveScene();

            //Load the game scene you want to use 
            yield return SceneManager.LoadSceneAsync("TestScene", LoadSceneMode.Additive);

            //After it is loaded, set the scene as Active 
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestScene"));


            // -------- ASSERT TEST -------- 

            // Assert taht the game scene has been set to active. If not, an exception will be thrown. 
            Assert.IsTrue(SceneManager.GetActiveScene().name == "TestScene");


            // -------- CLEAN UP -------- 
            
            //Set the active scene back to the test scene to close the test 
            SceneManager.SetActiveScene(originalScene);

            //Clean up the game scene 
            yield return SceneManager.UnloadSceneAsync("TestScene");
        }
        

    }
}
