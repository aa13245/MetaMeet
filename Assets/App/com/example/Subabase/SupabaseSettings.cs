using UnityEngine;

namespace com.example
{
	[CreateAssetMenu(fileName = "Supabase", menuName = "Supabase/Supabase Settings", order = 1)]
	public class SupabaseSettings : ScriptableObject
	{
		public string SupabaseURL = "https://oqjvbkeaoldlbllqbozf.supabase.co";
		public string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im9xanZia2Vhb2xkbGJsbHFib3pmIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MjQ1MDQzMjAsImV4cCI6MjA0MDA4MDMyMH0.jwoJPsL34HKh94O-moO_i8kCj2EFqXOLG5KI9l7IYRw";
	}
}
