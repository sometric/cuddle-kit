namespace CuddleKit.Reflection
{
	[System.Flags]
	public enum MemberAccess
	{
		Public = 0b1,
		NonPublic = 0b10,
		ReadOnly = 0b100,
		WriteOnly = 0b1000
	}
}
