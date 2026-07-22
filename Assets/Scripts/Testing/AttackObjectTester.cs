using UnityEngine;
using AttackSystem;
using Unity.VisualScripting;

public class AttackObjectTester : MonoBehaviour
{
    [Header("Transform")]
    public Transform source_transform;
    public Transform target_transform;
    public Transform output_transform;
    public Vector2 vfx_offset;


    [Header("Attack Type")]
    [SerializeReference] AttackObject attackObject = new Projectile();
    void Awake()
    {
        if (!source_transform) {source_transform = this.transform;}
        if (!output_transform) {output_transform = this.transform;}
    }
    public void Attack()
    {
        TargetData target_data = new TargetData(source_transform.position, target_transform.position, output_transform.position, vfx_offset);
        attackObject.Attack(target_data);
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
        if (attackObject == null)
        {
            attackObject = new Projectile();
        }
        if (attackObject.instance != null)
        {
            if (attackObject.GetSpecificAttackObject() != attackObject.GetType())
            {
                attackObject = attackObject.SmartRecast();
            }
        }
    }
}
