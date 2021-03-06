using UnityEngine;
using System.Collections;

namespace Completed
{
    //Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
    public class Enemy : MovingObject
    {
        //The amount of food points to subtract from the player when attacking.
        public int playerDamage;
        [SerializeField] int health;

        //Variable of type Animator to store a reference to the enemy's Animator component.
        private Animator animator;
        //Transform to attempt to move toward each turn.
        private Transform target;

        //Sprite renderer for orientation mapping
        private SpriteRenderer spriteRenderer;

        

        //Start overrides the virtual Start function of the base class.
        protected override void Start()
        {
            //Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
            //This allows the GameManager to issue movement commands.
            GameManager.instance.AddEnemyToList(this);

            //Get and store a reference to the attached Animator component.
            animator = GetComponent<Animator>();

            //Get sprite renderer reference
            spriteRenderer = GetComponent<SpriteRenderer>();

            //Find the Player GameObject using it's tag and store a reference to its transform component.
            target = GameObject.FindGameObjectWithTag("Player").transform;

            //Call the start function of our base class MovingObject.
            base.Start();
        }


        //Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
        //See comments in MovingObject for more on how base AttemptMove function works.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //Call the AttemptMove function from MovingObject.
            base.AttemptMove<T>(xDir, yDir);
        }


        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy()
        {
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            float xDiff = target.position.x - transform.position.x;
            float yDiff = target.position.y - transform.position.y;

            float RNG = Random.Range(0f, 1f);


            //  Start by choosing the correct direction and axis of movement towards the player
            if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
            {
                xDir = target.position.x > transform.position.x ? 1 : -1;
            }
            else
            {
                yDir = target.position.y > transform.position.y ? 1 : -1;
            }


            //  10% chance to just pick the wrong axis of movement
            if (RNG > .75)
            {
                if (xDir == 0)
                {
                    xDir = target.position.x > transform.position.x ? 1 : -1;
                    yDir = 0;
                }
                else
                {
                    xDir = 0;
                    yDir = target.position.y > transform.position.y ? 1 : -1;
                }
            }


            // 10% chance to go the wrong direction
            if (RNG < .1)
            {
                xDir *= -1;
                yDir *= -1;
            }

            //Trigger the walking animation
            animator.SetTrigger("enemyWalk");

            //Check the direction of player movement, and orient sprite accordingly
            if (xDir > 0.01f)
                spriteRenderer.flipX = false;
            else if (xDir < -0.01f)
                spriteRenderer.flipX = true;

            //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            AttemptMove<Player>(xDir, yDir);
        }


        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        protected override void OnCantMove<T>(T component)
        {
            //Declare hitPlayer and set it to equal the encountered component.
            Player hitPlayer = component as Player;

            //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            hitPlayer.LoseHealth(playerDamage);

            //Set the attack trigger of animator to trigger Enemy attack animation.
            animator.SetTrigger("enemyAttack");

        }

        public void takeDamage(int dmg)
        {
            health -= dmg;
            if(health < 1)
            {
                GameManager.instance.RemoveEnemyFromList(this);
                Destroy(gameObject);
            }
        }
    }
}