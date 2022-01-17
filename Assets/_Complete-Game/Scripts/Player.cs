using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.

		private Text scoreText;
		private Text healthText;
		[SerializeField] int health = 100;                           //Used to store player food points total during level.
		[SerializeField] int score = 0;
		[SerializeField] int whipDamage = 2;
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int food;                           //Used to store player food points total during level.
		private bool gameOver = false;				//Used for displaying animation of player death.
		public AudioClip whip_0;
		public AudioClip whip_1;

		private SpriteRenderer spriteRenderer;

		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();

			//Get a component reference to the Player's sprite renderer
			spriteRenderer = GetComponent<SpriteRenderer>();

			scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
			healthText = GameObject.Find("HealthText").GetComponent<Text>();

			score = int.Parse(scoreText.text);
			health = int.Parse(healthText.text.Split(' ')[1]);

			GameManager gameManager = GameManager.instance;
			score = gameManager.playerScore;
			health = gameManager.playerHealth;
			scoreText.text = score.ToString();
			healthText.text = "Health: " + health.ToString();

			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{

		}
		
		
		private void Update ()
		{
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.
			
			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//Check if moving horizontally, if so set vertical to zero.
			if(horizontal != 0)
			{
				vertical = 0;
			}

			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//Check the direction of player movement, and orient sprite accordingly
				if (horizontal > 0.01f)
					spriteRenderer.flipX = false;
				else if (horizontal < -0.01f)
					spriteRenderer.flipX = true;

				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove<Wall> (horizontal, vertical);
			}

			if (!base.isMoving)
				animator.SetTrigger("playerIdle");
			else if (base.isMoving)
				animator.SetTrigger("playerWalk");

			if (Input.GetKeyDown("space"))
			{
				StartCoroutine(whip());
			}

			if(Input.GetKeyDown(KeyCode.Escape))
			{
				health = 0;
			}

			if (Input.GetKeyDown("left shift"))
				animator.SetTrigger("playerCrouch");

			if(gameOver)
			{
				StartCoroutine(endGame());
			}
		}


			IEnumerator endGame()
		{
				if(Input.anyKeyDown)
				{
					yield return new WaitForSeconds(2f);
					GameManager.instance.QuitGame();
				}	
		}


		IEnumerator whip()
		{
				animator.SetTrigger("playerWhip");
				SoundManager.instance.RandomizeSfx(whip_0, whip_1);
				yield return new WaitForSeconds(0.3f);
				hitEnemies();		
		}

		
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			base.AttemptMove <T> (xDir, yDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
			
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver();
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			//GameManager.instance.playersTurn = false;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			//animator.SetTrigger ("playerPuncah");
		}
		
		private void hitEnemies()
		{			
			RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 1.0f, Vector2.left, Mathf.Infinity);
			foreach(RaycastHit2D hit in hits)
			{
				print(hit.transform.name);
				if(hit.collider.tag == "Enemy")
				{
					Enemy target = hit.collider.transform.GetComponent<Enemy>();
					target.takeDamage(whipDamage);
					return;
				}
			}
		}

		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit")
			{

				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);
				
				//Disable the player object since level is over.
				enabled = false;
			} else if( other.tag == "Treasure")
			{
				score += 1;
				scoreText.text = score.ToString();
				Destroy(other.gameObject);
			} else if( other.tag == "Food")
			{
				health += 10;
				health = Mathf.Clamp(health, 0, 100);
				healthText.text = "Health: " + health.ToString();
				Destroy(other.gameObject);
			}
		}

		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseHealth(int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			//animator.SetTrigger("playerHit");

			//Subtract lost food points from the players total.
			animator.SetTrigger("playerDamage");
			health -= loss;
			health = Mathf.Clamp(health, 0, 100);
			healthText.text = "Health: " + health.ToString();

			//Check to see if game has ended.
			CheckIfGameOver();
		}

		//Restart reloads the scene when called.
		private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
			//and not load all the scene object in the current scene.
			GameManager gameManager = GameManager.instance;
			gameManager.playerScore = score;
			gameManager.playerHealth = health;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		
		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			if (health == 0)
			{
				gameOver = true;
				animator.SetTrigger("playerDeath");
				SoundManager.instance.musicSource.Stop();
				StartCoroutine(WaitForDeathAnimation());
			}
		}

		private static IEnumerator WaitForDeathAnimation()
		{
			yield return new WaitForSeconds(2.5f);
			GameManager.instance.GameOver();			
		}

	}
}

