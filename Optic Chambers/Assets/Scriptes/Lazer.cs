using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Lazer : MonoBehaviour
{
    [FormerlySerializedAs("RayDistance")] [SerializeField] private float rayDistance = 10;
    [FormerlySerializedAs("ResonatorBoostPower")] [SerializeField] private float resonatorBoostPower = 10;
    private LayerMask _laserLayer;
    private int _numberOfLaser;

    private List<LaserObject> _lasers;
    [FormerlySerializedAs("m_lineRenderer")] public LineRenderer mLineRenderer;
    public Transform laserFirePoint;
    private Material _laserMaterial;
    private List<GameObject> _lineRendererToDestroy;

    private static UIManager _uiManager;
    private static readonly int Activated = Animator.StringToHash("Activated");

    private static int levelwin = 0;
    private void Awake()
    {
        mLineRenderer.SetPosition(0, laserFirePoint.position);
        _laserMaterial = mLineRenderer.material;
        _laserLayer |= (1 << LayerMask.NameToLayer("Default"));
        _lineRendererToDestroy = new List<GameObject>();
    }

    private void Start()
    {
        _uiManager = UIManager.Instance;
    }

    void ShootLaser(int laserNumber, float rayPower, Vector2 laserDirectorVector, Vector2 startPoint)
    {
        RaycastHit2D hit = Physics2D.Raycast(startPoint, laserDirectorVector, rayPower, _laserLayer);
        if (hit)
        {
                switch (hit.transform.tag)
                {
                    case "Mirror":
                        //Debug.Log("mirror");
                        ComputeMirror(laserNumber, laserDirectorVector, hit, rayPower-hit.distance);
                        break;
                    case "ResonatorBoost":
                        //Debug.Log("resonatorBoost");
                        ComputeResonatorBoost(laserNumber, laserDirectorVector, hit, rayPower-hit.distance);
                        break;
                    case "WinTarget":
                        if (levelwin == 0)
                        {       
                            Debug.Log("Win");
                            levelwin = 1;
                            _uiManager.LevelVictory();
                        }
                        _lasers[laserNumber].addStep(hit.point);
                        break;
                    case "ColorChanger":
                        Debug.Log("ColorChanger");
                        // Laser Exit 1
                        GameObject childLineRenderer = new GameObject();
                        _lineRendererToDestroy.Add(childLineRenderer);
                        childLineRenderer.AddComponent<LineRenderer>();
                        childLineRenderer.transform.parent = hit.transform;
                        childLineRenderer.transform.position = childLineRenderer.transform.parent.position;
                        childLineRenderer.layer = 10;
                        CopyLineRendererSetting(childLineRenderer.GetComponent<LineRenderer>(), mLineRenderer);
                        _lasers.Add(new LaserObject(childLineRenderer.GetComponent<LineRenderer>(),_laserMaterial));

                        String ColorChangerName = hit.transform.gameObject.name;

                        switch (ColorChangerName)
                        {
                            case "Blue":
                                _lasers[_numberOfLaser].setColor(Color.blue);
                                break;
                            case "Red":
                                _lasers[_numberOfLaser].setColor(Color.red);
                                break;
                            case "Green":
                                _lasers[_numberOfLaser].setColor(Color.green);
                                break;
                            default:
                                _lasers[_numberOfLaser].setColor(Color.red);
                                break;
                        }
                        ShootLaser(_numberOfLaser, rayPower, laserDirectorVector, hit.point+laserDirectorVector.normalized*0.01f);
                        _numberOfLaser++;
                        
                        _lasers[laserNumber].addStep(hit.point);
                        break;
                    case "Button":
                        //Debug.Log("Button");
                        _lasers[laserNumber].addStep(hit.point);
                        
                        // Check ButtonColor
                        ButtonManager button = hit.transform.GetComponent<ButtonManager>();
                        if (button.isRightColor(_lasers[laserNumber]))
                        {
                            button.Activate();
                        }
                        else
                        {
                            Debug.Log("Wrong Color");
                        }
                        
                        
                        break;
                    case "ResonatorJumeling":
                        //Debug.Log("Jumeling");
                        ComputeResonatorTwin(hit, rayPower-hit.distance);
                        _lasers[laserNumber].addStep(hit.point);
                        break;
                    case "SplitEntrace":
                        //Debug.Log("Split");
                        ComputeSplit(hit, rayPower-hit.distance);
                        _lasers[laserNumber].addStep(hit.point);
                        break;
                    default:
                        _lasers[laserNumber].addStep(hit.point);
                        break;
                }
        }
        else
        {
            _lasers[laserNumber].addStep(startPoint + laserDirectorVector * rayPower);
        }
        
    }
    
    void ComputeMirror(int laserNumber, Vector2 inputLaser, RaycastHit2D hit, float rayPower)
    {
        // Compute new laser angle
        Vector2 laserDirectorVector = Vector2.Reflect(inputLaser, hit.normal);
        
        // Compute new laser starting Position

        Vector2 newStartingPoint = hit.point + laserDirectorVector.normalized*0.1f;
        
        // Add step to list
        _lasers[laserNumber].addStep(hit.point);
        
        // Shoot new laser
        //Debug.Log("mirror - "+laserDirectorVector);
        ShootLaser(laserNumber, rayPower, laserDirectorVector, newStartingPoint);
    }
    
    void ComputeSplit(RaycastHit2D hit, float rayPower)
    {
        // Recover Exit point
        Vector2 exitPoint1 = hit.transform.GetChild(0).transform.position;
        // print(hit.transform.GetChild(0).transform.position);
        Vector2 exitPoint2 = hit.transform.GetChild(1).transform.position;
        // print(hit.transform.GetChild(1).transform.position);
        
        
        // Craft exit Director Vector
        Vector2 directorVectorExit1 = (Vector2.up + Vector2.right).normalized;
        Vector2 directorVectorExit2 = (Vector2.down + Vector2.right).normalized;

        // Laser Exit 1
        GameObject childLineRenderer = new GameObject();
        _lineRendererToDestroy.Add(childLineRenderer);
        childLineRenderer.AddComponent<LineRenderer>();
        childLineRenderer.transform.parent = hit.transform.GetChild(0).transform;
        childLineRenderer.transform.position = childLineRenderer.transform.parent.position;
        childLineRenderer.layer = 10;
        CopyLineRendererSetting(childLineRenderer.GetComponent<LineRenderer>(), mLineRenderer);
        _lasers.Add(new LaserObject(childLineRenderer.GetComponent<LineRenderer>(),_laserMaterial));
        ShootLaser(_numberOfLaser, rayPower/2, directorVectorExit1, exitPoint1);
        _numberOfLaser++;

        // Laser Exit 2
        GameObject childLineRenderer2 = new GameObject();
        _lineRendererToDestroy.Add(childLineRenderer2);
        childLineRenderer2.AddComponent<LineRenderer>();
        childLineRenderer2.transform.parent = hit.transform.GetChild(1).transform;
        childLineRenderer2.transform.position = childLineRenderer2.transform.parent.position;
        childLineRenderer2.layer = 10;
        CopyLineRendererSetting(childLineRenderer2.GetComponent<LineRenderer>(), mLineRenderer);
        _lasers.Add(new LaserObject(childLineRenderer2.GetComponent<LineRenderer>(), _laserMaterial));
        ShootLaser(_numberOfLaser, rayPower/2, directorVectorExit2, exitPoint2);
        _numberOfLaser++;

    }
    
    // =================== Resonator =================== 

    void ComputeResonatorBoost(int laserNumber, Vector2 entry, RaycastHit2D hit, float rayPower)
    {
        Vector2 newStartingPoint = hit.point + entry.normalized;
        
        // Add step to list
        _lasers[laserNumber].addStep(hit.point);
        
        // Animation
        Animator boostAnimator = hit.transform.GetComponent<Animator>();
        boostAnimator.SetBool(Activated, true);
        hit.transform.GetComponent<AnimationTriggerManagment>().Hit();
        
        // Shoot new laser
        ShootLaser(laserNumber, rayPower+resonatorBoostPower, entry, newStartingPoint);
    }

    void ComputeResonatorTwin(RaycastHit2D hit, float rayPower)
    {
        // Recover Exit point
        Vector2 exitPointStrong = hit.transform.GetChild(0).transform.position;
        // print(hit.transform.GetChild(0).transform.position);
        // Debug.Log(hit.transform.GetChild(0).name);
        Vector2 exitPointWeak = hit.transform.GetChild(1).transform.position;
        // Debug.Log(hit.transform.GetChild(1).name);
        
        // Craft exit Director Vector
        Vector2 directorVector = Vector2.right;
        
        // Animation
        Transform sprite = hit.transform.GetChild(2);
        Animator boostAnimator = sprite.GetComponent<Animator>();
        boostAnimator.SetBool(Activated, true);
        sprite.GetComponent<AnimationTriggerManagment>().Hit();
        
        // Laser Strong
        GameObject childLineRenderer = new GameObject();
        _lineRendererToDestroy.Add(childLineRenderer);
        childLineRenderer.AddComponent<LineRenderer>();
        childLineRenderer.transform.parent = hit.transform.GetChild(0).transform;
        childLineRenderer.transform.position = childLineRenderer.transform.parent.position;
        childLineRenderer.layer = 10;
        CopyLineRendererSetting(childLineRenderer.GetComponent<LineRenderer>(), mLineRenderer);
        _lasers.Add(new LaserObject(childLineRenderer.GetComponent<LineRenderer>(),_laserMaterial));
        ShootLaser(_numberOfLaser, rayPower, directorVector, exitPointStrong);
        _numberOfLaser++;
        
        //Laser Weak
        GameObject childLineRendererWeak = new GameObject();
        _lineRendererToDestroy.Add(childLineRendererWeak);
        childLineRendererWeak.AddComponent<LineRenderer>();
        childLineRendererWeak.transform.parent = hit.transform.GetChild(1).transform;
        childLineRendererWeak.layer = 10;
        childLineRendererWeak.transform.position = childLineRendererWeak.transform.parent.position;
        CopyLineRendererSetting(childLineRendererWeak.GetComponent<LineRenderer>(), mLineRenderer, 0.5f);
        
        // // Calculate symmetry axis & point
        Vector2 strongWeakVector2 = exitPointWeak - exitPointStrong;
        Vector2 symmetryAxisPoint = exitPointStrong + strongWeakVector2 / 2;
        
        // Set Twin
        _lasers[_numberOfLaser-1].setWeekTwin(strongWeakVector2.Perpendicular1(), symmetryAxisPoint, childLineRendererWeak.GetComponent<LineRenderer>());

    }

    // =================== Cuves ===================
    
    
    
    void CopyLineRendererSetting(LineRenderer lr1, LineRenderer lr2, float mult = 1)
    {
        lr1.widthCurve = lr2.widthCurve;
        lr1.startWidth = lr2.startWidth * mult;
        lr1.endWidth = lr2.endWidth * mult;
    }
    
    
    /**
     * n0 origin environment
     * n1 exit environment
     * i origin angle
     */
    float ComputeRefractionAngle(float n0, float n1, float i)
    {
        return Mathf.Asin((n0 / n1) * Mathf.Sin(i));
    }
    
    public static Vector2 Rotate(Vector2 v, float delta) {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }


    // Update is called once per frame
    void Update()
    {
        // Debug.Log("=======================");
        _lasers = new List<LaserObject>();
        DestroyLineRenderer();
        _lasers.Add(new LaserObject(mLineRenderer, _laserMaterial));
        var position = laserFirePoint.position;
        _lasers[0].addStep(position);
        _numberOfLaser = 1;
        ShootLaser(0, rayDistance, laserFirePoint.transform.right, position);
        
        //Debug.Log("Laser Number " + NumberOfLaser);
        //Debug.Log("Laser In List " + Lasers.Count);
        Draw2DRay();

    }
    
    void Draw2DRay()
    {
        foreach (LaserObject laser in _lasers)
        {
            laser.Draw();
        }
    }

    void DestroyLineRenderer()
    {
        if (_lineRendererToDestroy.Count!=0)
        {
            foreach (GameObject lineRend in _lineRendererToDestroy)
            {
                Destroy(lineRend);
            }
        }
        
    }

    public static Vector2 CheckCollision(Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 laserDirectorVector = endPoint - startPoint;
        RaycastHit2D hit = Physics2D.Raycast(startPoint, laserDirectorVector.normalized, laserDirectorVector.magnitude);
        if (hit)
        {
            switch (hit.transform.tag)
            {
                case "WinTarget":
                    Debug.Log("Win");
                    levelwin = 1;
                    _uiManager.LevelVictory();
                    return hit.point;
                default:
                    return hit.point;
            }
        }
        return endPoint;
    }
}
