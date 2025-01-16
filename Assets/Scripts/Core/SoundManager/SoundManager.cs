using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using Zenject;

namespace Core.SoundManager
{
	/// <summary>
	/// Sound manager.
	/// </summary>
	[DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
	public class SoundManager : MonoBehaviour, ISoundManager
	{
		private const string MusicVol = "MusicVolume";
		private const string SoundVol = "SoundVolume";
		private const float MusicVolMin = -80f;
		private const float MusicVolMax = -10f;
		private const float SoundVolMin = -80f;
		private const float SoundVolMax = 0f;
		private const float MusicOffDuration = 0.5f;

		private AudioSource _musicAudioSource;
		private readonly Lazy<Dictionary<string, AudioClip>> _clipsMap;

		private Tween _tween;

		private bool? _musicIsOn;
		private bool? _soundIsOn;

		private float? _musicVolume;
		private float? _soundVolume;

		private readonly List<AudioSource> _soundPool = new();

		[SerializeField] private AudioMixer _audioMixer;
		[SerializeField, Header("Prefabs")] private AudioSource _soundSourcePrefab;
		[SerializeField, Header("Sound list")] private List<AudioClip> _clips;

		[Inject] private readonly DiContainer _container;

		public SoundManager()
		{
			_clipsMap = new Lazy<Dictionary<string, AudioClip>>(() => (_clips ?? new List<AudioClip>())
				.Select(clip => (clip.name, clip))
				.GroupBy(tuple => tuple.name)
				.ToDictionary(tuples => tuples.Key, tuples => tuples.First().clip)
			);
		}

		private void Awake()
		{
			_musicAudioSource = GetComponent<AudioSource>();
			Assert.IsTrue(_musicAudioSource && _audioMixer, "AudioSource and Audio Mixer reference must have.");
		}

		private void Start()
		{
			if (!_musicIsOn.HasValue)
			{
				MusicIsOn = true;
			}

			if (!_soundIsOn.HasValue)
			{
				SoundIsOn = true;
			}

			if (!_soundVolume.HasValue)
			{
				SoundVolume = 1f;
			}

			if (!_musicVolume.HasValue)
			{
				MusicVolume = 1f;
			}
		}

		private void OnDestroy()
		{
			_tween?.Kill(true);
		}

		// ISoundManager

		public void PlaySound(string soundName, float? delaySec = null)
		{
			if (!SoundIsOn)
			{
				return;
			}

			var clip = _clipsMap.Value.GetValueOrDefault(soundName);
			if (!clip)
			{
				Debug.LogErrorFormat("There is no audio clip with the name {0}.", soundName);
				return;
			}

			var src = _soundPool.FirstOrDefault(source => !source.isPlaying);
			if (!src)
			{
				Assert.IsTrue(_soundSourcePrefab, "Sound source prefab must have.");
				src = _container.InstantiatePrefab(_soundSourcePrefab, transform).GetComponent<AudioSource>();
				_soundPool.Add(src);
			}

			src.clip = clip;

			if (delaySec is > 0)
			{
				src.PlayDelayed(delaySec.Value);
			}
			else
			{
				src.Play();
			}
		}

		public void PlayMusic(string musicName, float fadeDurationSec = 0.75f)
		{
			if (musicName == CurrentMusicName)
			{
				return;
			}

			if (_musicAudioSource.clip)
			{
				_tween?.Kill();
				_tween = DOTween.To(() => MusicVolume, vol => MusicVolume = vol, 0f, MusicOffDuration)
					.SetEase(Ease.InQuad)
					.OnComplete(() =>
					{
						_tween = null;
						_musicAudioSource.Stop();
						_musicAudioSource.clip = null;

						StartMusic(musicName, fadeDurationSec);
					});
			}
			else
			{
				StartMusic(musicName, fadeDurationSec);
			}
		}

		public bool SoundIsOn
		{
			get => _soundIsOn ?? false;
			set
			{
				if (value == _soundIsOn)
				{
					return;
				}

				_soundIsOn = value;
				if (!value)
				{
					_soundPool.ForEach(source => source.Stop());
				}
			}
		}

		public bool MusicIsOn
		{
			get => _musicIsOn ?? false;
			set
			{
				if (value == _musicIsOn)
				{
					return;
				}

				_musicIsOn = value;
				_musicAudioSource.enabled = value;
			}
		}

		public float MusicVolume
		{
			get => _musicVolume ?? (_audioMixer.GetFloat(MusicVol, out var v) ? Mathf.Clamp01((v - MusicVolMin) / (MusicVolMax - MusicVolMin)) : 0f);
			set
			{
				if (value.Equals(_musicVolume))
				{
					return;
				}

				_musicVolume = value;
				_audioMixer.SetFloat(MusicVol, Mathf.Lerp(MusicVolMin, MusicVolMax, value));
			}
		}

		public float SoundVolume
		{
			get => _soundVolume ?? (_audioMixer.GetFloat(SoundVol, out var v) ? Mathf.Clamp01((v - SoundVolMin) / (SoundVolMax - SoundVolMin)) : 0f);
			set
			{
				if (value.Equals(_soundVolume))
				{
					return;
				}

				_soundVolume = value;
				_audioMixer.SetFloat(SoundVol, Mathf.Lerp(SoundVolMin, SoundVolMax, value));
			}
		}

		// \ISoundManager

		private string CurrentMusicName => _musicAudioSource.clip != null ? _musicAudioSource.clip.name : null;

		private void StartMusic(string musicName, float fadeDuration)
		{
			Assert.IsFalse(_musicAudioSource.clip, "Previous music must stopped first.");
			var clip = _clipsMap.Value.GetValueOrDefault(musicName);
			if (!clip)
			{
				Debug.LogErrorFormat("There is no audio clip with the name {0}.", musicName);
				return;
			}

			if (string.IsNullOrEmpty(musicName))
			{
				_musicAudioSource.Stop();
			}
			else
			{
				_musicAudioSource.clip = clip;

				_tween?.Kill();
				var currentVolume = _musicVolume ?? 1f;
				if (fadeDuration > 0)
				{
					MusicVolume = 0f;
					_tween = DOTween.To(() => MusicVolume, vol => MusicVolume = vol, currentVolume, fadeDuration)
						.SetEase(Ease.OutQuad)
						.OnComplete(() => _tween = null);
				}
				else
				{
					MusicVolume = currentVolume;
					_tween = null;
				}

				_musicAudioSource.Play();
			}
		}
	}
}