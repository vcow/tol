using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace StartScene
{
	[CreateAssetMenu(fileName = "PlayerNameInputValidator", menuName = "TextMeshPro/Input Validators/Player Name Input Validator", order = 100)]
	public class PlayerNameInputValidator : TMP_InputValidator
	{
		private readonly Regex _rx = new(@"^[\wа-яА-Я]+$", RegexOptions.Singleline);

		public override char Validate(ref string text, ref int pos, char ch)
		{
			var newText = text + ch;
			if (_rx.Match(newText).Success)
			{
				text = newText;
				pos += 1;
				return ch;
			}

			return '\0';
		}
	}
}