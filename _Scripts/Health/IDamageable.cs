using KinematicCharacterController.Examples;

public interface IDamageable
{
    void TakeDamageServerRpc(
        float damage, 
        ShakeData cameraShakeData = default, 
        DamageHitType hitType = DamageHitType.None, 
        DamageHurtType hurtType = DamageHurtType.Hurt);
}
