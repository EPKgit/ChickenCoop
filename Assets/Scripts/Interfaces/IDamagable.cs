using UnityEngine;

public interface IDamagable
{
	/// <summary>
	/// Damage the object. Implements on a per object basis
	/// </summary>
	/// <param name="delta">The amount of damage</param>
	/// <param name="localSource">The actual gameobject that is applying damage. May be equal to the overallSource</param>
	/// <param name="overallSource"> The source of the damage overall. For example a projectiles local source may be the projectile itself, while the overall source would be the player that spawned it.</param>
	void Damage(float delta, GameObject localSource, GameObject overallSource = null);
}