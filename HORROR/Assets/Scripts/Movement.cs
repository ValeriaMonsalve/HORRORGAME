using UnityEngine;
public class Player : MonoBehaviour
{
    public float velMov = 5f;
    public float velRot = 100f;
    float mov, gir;
    private bool sonidoMotor = false;

    private void Update()

    {
        float axisV = Input.GetAxis("Vertical");
        float axisH = Input.GetAxis("Horizontal");

        if (axisV != 0)
        {
            Mover();
        }

        if (axisH != 0)
        {
            Girar();
        }

        if (axisV != 0 || axisH != 0)

        {
            if (!sonidoMotor)
            {
                AudioManager.instance.Play("Respirar");
                sonidoMotor = true;
            }

        }

        else

        {
            if (sonidoMotor)
            {
                AudioManager.instance.Play("Respirar");
                sonidoMotor = false;
            }
        }
    }
    void Mover()
    {
        mov = Input.GetAxis("Vertical") * velMov * Time.deltaTime;
        transform.Translate(0, 0, mov);
    }

    void Girar()
    {
        gir = Input.GetAxis("Horizontal") * velRot * Time.deltaTime;
        transform.Rotate(0, gir, 0);
    }
}