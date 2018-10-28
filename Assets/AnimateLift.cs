using UnityEngine;

public class AnimateLift : MonoBehaviour
{
  // public Vector3 pos1;
  // public Vector3 pos2;

  public float top;
  public float bottom;
  public float speed = 1.0f;

  public AudioSource liftsound;

  private Vector3 pos1;
  private Vector3 pos2;

  public bool goingDown = false;

  private bool goingUp = false;

public Transform player;
  void OnEnable()
  {
    pos1 = new Vector3(gameObject.transform.localPosition.x, top, gameObject.transform.localPosition.z);
    pos2 = new Vector3(gameObject.transform.localPosition.x, bottom, gameObject.transform.localPosition.z);
  }

  public bool IsMoving()
  {
      return goingDown || goingUp;
  }

  public bool IsAtTop()
  {
      return player.transform.position.y >= top;
    //   return gameObject.transform.localPosition.y >= top;
  }

  public bool IsAtBottom()
  {
      return gameObject.transform.localPosition.y <= bottom;
  }

  public void GoDown()
  {
    if (!IsMoving())
        goingDown = true;
        liftsound.Play();
  }

  public void GoUp()
  {
    if (!IsMoving())
        goingUp = true;
        liftsound.Play();
  }

  void Update()
  {
    if (IsAtTop() && goingUp)
    {
      goingUp = false;
    }

    if (IsAtBottom() && goingDown)
    {
      goingDown = false;
    }

    if (goingDown)
    {
      transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos2, Time.deltaTime * speed);
      player.position = Vector3.MoveTowards(player.position, new Vector3(player.position.x, bottom, player.position.z), Time.deltaTime * speed);

    }
    else if (goingUp)
    {
      transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos1, Time.deltaTime * speed);
      player.position = Vector3.MoveTowards(player.position, new Vector3(player.position.x, top, player.position.z), Time.deltaTime * speed);
    }

  }
}
