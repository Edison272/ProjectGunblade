using UnityEngine;
using AttackSystem;
using Unity.VisualScripting;

public class AttackTypeTester : MonoBehaviour
{
    [Header("Transform")]
    public Transform source_transform;
    public Transform target_transform;
    public Transform output_transform;
    public Vector2 vfx_offset;


    [Header("Attack Type")]
    [SerializeReference] AttackType attackType = new Projectile();
    void Awake()
    {
        if (!source_transform) {source_transform = this.transform;}
        if (!output_transform) {output_transform = this.transform;}
    }
    public void Attack()
    {
        AttackTarget target_data = new AttackTarget(source_transform.position, target_transform.position, output_transform.position, vfx_offset);
        attackType.Attack(target_data);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValidate()
    {
        if (attackType == null)
        {
            attackType = new Projectile();
        }
        if (attackType.instance != null)
        {
            if (attackType.GetSpecificAttackType() != attackType.GetType())
            {
                attackType = attackType.SmartRecast();
            }
        }
    }
}
