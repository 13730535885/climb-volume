using UnityEngine;

namespace ClimbDemo
{
    public class ClimbDemoHUD : MonoBehaviour
    {
        public ClimbDemoController player;
        private GUIStyle title;
        private GUIStyle body;

        private void OnGUI()
        {
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
                title.normal.textColor = new Color(0.95f, 0.92f, 0.84f);
                body = new GUIStyle(GUI.skin.label) { fontSize = 15 };
                body.normal.textColor = new Color(0.72f, 0.76f, 0.80f);
            }

            GUI.Label(new Rect(24, 20, 700, 30), "CLIMB VOLUME  /  standalone demo", title);
            GUI.Label(new Rect(24, 50, 760, 24), "A / D  move        W / UP  climb        walk off the top edge to fall", body);
            if (player != null)
            {
                string state = player.IsClimbing ? "CLIMBING" : (player.IsGrounded ? "GROUNDED" : "AIRBORNE");
                GUI.Label(new Rect(24, 76, 400, 24), "state: " + state, body);
            }
        }
    }
}
