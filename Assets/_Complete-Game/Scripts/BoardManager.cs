using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed
	
{
	
	public class BoardManager : MonoBehaviour
	{
		// Using Serializable allows us to embed a class with sub properties in the inspector.
		[Serializable]
		public class Count
		{
			public int minimum; 			//Minimum value for our Count class.
			public int maximum; 			//Maximum value for our Count class.
			
			
			//Assignment constructor.
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}
		
		
		private int columns = 0; 										
		private int rows = 0;                                           
		private int initialColumns = 5; 									
		private int initialRows = 5;                                     
		[SerializeField] int levelScale = 2;		
		[SerializeField] Count wallCount = new Count (5, 9);						
		[SerializeField] Count foodCount = new Count (1, 3);						
		[SerializeField] Count treasureCount = new Count (5, 10);
		public GameObject exit;                                         
		public GameObject floorTile;
		public GameObject[] wallTiles;									
		public GameObject[] foodTiles;		
		public GameObject[] treasureTiles;								
		public GameObject[] enemyTiles;									
		public GameObject outerWallTile;										
		private Transform boardHolder;									
		private List <Vector3> gridPositions = new List <Vector3> ();	
		
		
		//Clears our list gridPositions and prepares it to generate a new board.
		void InitialiseList ()
		{
			//Clear our list gridPositions.
			gridPositions.Clear ();
			
			//Loop through x axis (columns).
			for(int x = 1; x < columns-1; x++)
			{
				//Within each column, loop through y axis (rows).
				for(int y = 1; y < rows-1; y++)
				{
					//At each index add a new Vector3 to our list with the x and y coordinates of that position.
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}
		
		
		//Sets up the outer walls and floor (background) of the game board.
		void BoardSetup (int level)
		{
			//Instantiate Board and set boardHolder to its transform.
			boardHolder = new GameObject ("Board").transform;

			columns = initialColumns + level * levelScale;
			rows = initialRows + level * levelScale;

			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
			for (int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
				for(int y = -1; y < rows + 1; y++)
				{
					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
					GameObject toInstantiate = floorTile;

					//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
					if (x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTile;
					
					//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					
					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
					instance.transform.SetParent (boardHolder);
				}
			}
		}
		
		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition (bool isEnemy = false)
		{
			//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];

			while (isEnemy && randomPosition.x < 1.5 || randomPosition.y < 1.5)
            {
				randomIndex = Random.Range(0, gridPositions.Count);
				randomPosition = gridPositions[randomIndex];
			}
			
			//Remove the entry at randomIndex from the list so that it can't be re-used.
			gridPositions.RemoveAt (randomIndex);
			
			//Return the randomly selected Vector3 position.
			return randomPosition;
		}
		
		void LayoutCornerMaze(GameObject[] tileArray)
		{			
			int gap = Random.Range(3,5);

			if(Random.Range(0f, 1f) <= -0.5)
			{
				int start = 2;
				while (start < columns - 1)
				{
					int row = start, col = start;

					int openSpace = Random.Range(start + 1, columns);

					bool spaceInRow = Random.Range(0f, 1f) < .5;

					for(int i = row; i < rows; i++)
					{
						if((i == openSpace || i == openSpace + 1 || i == openSpace -1) && spaceInRow) continue;
						GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
						Instantiate(tileChoice, new Vector3(col, i, 0), Quaternion.identity);
						gridPositions.Remove(new Vector3(col, i, 0));
					}

					for(int i = col + 1; i < columns; i++)
					{
						if((i == openSpace || i == openSpace + 1 || i == openSpace -1) && !spaceInRow ) continue;
						GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
						Instantiate(tileChoice, new Vector3(i, row, 0), Quaternion.identity);
						gridPositions.Remove(new Vector3(i, row, 0));
					}
					start += gap;
				}
			} else 
			{
				int start = columns - 2;
				while (start >  1)
				{
					int row = start, col = start;

					int openSpace = Random.Range(1, start);

					bool spaceInRow = Random.Range(0f, 1f) < .5;
					
					for(int i = row; i >= 0; i--)
					{
						if((i == openSpace || i == openSpace + 1 || i == openSpace -1) && spaceInRow) continue;
						GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
						Instantiate(tileChoice, new Vector3(col, i, 0), Quaternion.identity);
						gridPositions.Remove(new Vector3(col, i, 0));
					}

					for(int i = col - 1; i >= 0; i--)
					{
						if((i == openSpace || i == openSpace + 1 || i == openSpace -1) && !spaceInRow) continue;
						GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
						Instantiate(tileChoice, new Vector3(i, row, 0), Quaternion.identity);
						gridPositions.Remove(new Vector3(i, row, 0));
					}
					start -= gap;
				}
			}
		}
		
		void LayoutVerticalMaze(GameObject[] tileArray)
		{
			for(int col = 1; col < columns - 1; col += 3)
			{
				int openRow = Random.Range(1, rows-1);
				for(int row = 0; row <= rows - 1; row++)
				{
					if (row == openRow || row == openRow - 1 || row == openRow + 1) continue;

					GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
					Instantiate(tileChoice, new Vector3(col, row, 0), Quaternion.identity);
					gridPositions.Remove(new Vector3(col, row, 0));

				}
			}
		}

		void LayoutHorizontalMaze(GameObject[] tileArray)
		{
			
			for(int col = 2; col < columns - 1; col += 3)
			{
				int openRow = Random.Range(1, rows-1);
				for(int row = 0; row <= rows - 1; row++)
				{
					if (row == openRow || row == openRow - 1 || row == openRow + 1) continue;

					GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
					Instantiate(tileChoice, new Vector3(row, col, 0), Quaternion.identity);
					gridPositions.Remove(new Vector3(col, row, 0));
				}
			}
		}

		//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum, int level, bool isEnemy = false)
		{
			//Choose a random number of objects to instantiate within the minimum and maximum limits
			int objectCount = Random.Range (minimum, maximum+1) + level * levelScale / 2;
			
			//Instantiate objects until the randomly chosen limit objectCount is reached
			for(int i = 0; i < objectCount; i++)
			{
				//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
				Vector3 randomPosition = RandomPosition(isEnemy);
				
				//Choose a random tile from tileArray and assign it to tileChoice
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				
				//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}
		
		
		//SetupScene initializes our level and calls the previous functions to lay out the game board
		public void SetupScene (int level)
		{
			//Creates the outer walls and floor.
			BoardSetup (level);
			
			//Reset our list of gridpositions.
			InitialiseList ();
			
			// coin flip to determine what the maze looks like.
			//Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
			float flip = Random.Range(0.0f, 1.0f);
			if(flip <= 10.33) LayoutCornerMaze(wallTiles);
			else if(flip < 0.25) LayoutVerticalMaze(wallTiles);
			else if(flip <= 0.66) LayoutHorizontalMaze(wallTiles);
			else LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum, level);
			
			LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum, 0);
			LayoutObjectAtRandom (treasureTiles, foodCount.minimum, foodCount.maximum, level*2);
			
			//Determine number of enemies based on current level number, based on a logarithmic progression
			int enemyCount = (int)Mathf.Log(level, 2f);
			
			//Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount, level, true);
			
			//Instantiate the exit tile in the upper right hand corner of our game board
			Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
		}
	}
}
