using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class collision_detection_m : MonoBehaviour
{
    // Start is called before the first frame update
    void OnCollisionEnter(Collision collision)
    {
        var csv = new StringBuilder();
        //Debug.Log("collision");
        //Debug.Log(collision.collider.name);
        //Debug.Log(collision.collider.transform.position);
        //Debug.Log("id" + gameObject.name.ToString());
        if (collision.gameObject.name.Contains("VehicleID"))
        {
            var id = gameObject.name.ToString();
            var pos_x = gameObject.transform.position.x.ToString();
            var pos_z = gameObject.transform.position.z.ToString();
            var col_id = collision.gameObject.name.ToString();
            var col_pos_x = collision.gameObject.transform.position.x.ToString();
            var col_pos_z = collision.gameObject.transform.position.z.ToString();
            var time = System.DateTime.Now.ToString();

            float colForce = collision.impulse.magnitude / Time.fixedDeltaTime;

            var newline = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", id, pos_x, pos_z, col_id, col_pos_x, col_pos_z, time, colForce);
            csv.AppendLine(newline);
            System.IO.File.AppendAllText("C:\\Users\\eoinb\\New Unity Project\\Assets\\SampleScenes\\Scripts\\3blow.csv", csv.ToString());
        }
    }
}
