public class Variables
{
  public string userID { get; set; }
  public string evisID { get; set; }
  public float timeStamp { get; set; }
  
  public float currentStateOfCharge { get; set; }
  public float energyConsumed { get; set; }
  public float energyUsage { get; set; }
  public float guesstimatedDistanceLeft { get; set; }

  public float speed { get; set; }
  public float distanceTraveled { get; set; }
  
  public float throttlePosition { get; set; }
  public float breakPosition { get; set; }
  public float steeringWheelRot { get; set; }

  public float xPosition { get; set; } // Horizontal
  public float yPosition { get; set; } // Vertical 
  public float zPosition { get; set; } // Horizontal
}