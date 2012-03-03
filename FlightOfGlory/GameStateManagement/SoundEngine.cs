using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace GameStateManagement
{
    public class SoundEngine
    {
        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;
        Cue[] soundEffects_cue;
        Cue[] music_cue;
        AudioListener listener;
        AudioEmitter emitter;

        public void Initialize()
        {
            audioEngine = new AudioEngine(@"Content\GameSounds\Sounds.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\GameSounds\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\GameSounds\Sound Bank.xsb");
            soundEffects_cue = new Cue[8];
            music_cue = new Cue[3];
            listener = new AudioListener();
            emitter = new AudioEmitter();
            soundEffects_cue[0] = soundBank.GetCue("afterburner");
            soundEffects_cue[1] = soundBank.GetCue("missile");
            soundEffects_cue[2] = soundBank.GetCue("bullet");
            soundEffects_cue[3] = soundBank.GetCue("UI_Click");
            soundEffects_cue[4] = soundBank.GetCue("engage_engines");
            soundEffects_cue[5] = soundBank.GetCue("explode");
            soundEffects_cue[6] = soundBank.GetCue("jet_explode");
            soundEffects_cue[7] = soundBank.GetCue("equip");
            music_cue[0] = soundBank.GetCue("bg01");
            music_cue[1] = soundBank.GetCue("bg02");
            music_cue[2] = soundBank.GetCue("bg03");
        }

        public void SetCategoryVolume(int sfx, int bgm)
        {
            AudioCategory sfxCategory, bgmCategory;
            sfxCategory = audioEngine.GetCategory("SoundEffects");
            bgmCategory = audioEngine.GetCategory("Music");

            sfxCategory.SetVolume(sfx / 5.0f);
            bgmCategory.SetVolume(bgm / 5.0f);
        }

        public void PlayWeaponToggle(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[7].IsPlaying)
            {
                soundEffects_cue[7] = soundBank.GetCue("equip");

                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[7].Apply3D(listener, emitter);

                soundEffects_cue[7].Play();
            }
        }

        public void PlayAfterburner(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[0].IsPlaying)
            {
                soundEffects_cue[0] = soundBank.GetCue("afterburner");

                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[0].Apply3D(listener, emitter);

                soundEffects_cue[0].Play();
            }
        }

        public void StopAfterburner()
        {
            soundEffects_cue[0].Stop(AudioStopOptions.Immediate);
        }

        public void PlayMissile(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[1].IsPlaying)
            {
                soundEffects_cue[1] = soundBank.GetCue("missile");
                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[1].Apply3D(listener, emitter);
                soundEffects_cue[1].Play();
            }
        }

        public void PlayBullet(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[2].IsPlaying)
            {
                soundEffects_cue[2] = soundBank.GetCue("bullet");
                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[2].Apply3D(listener, emitter);
                soundEffects_cue[2].Play();
            }
        }

        public void PlayUIClick()
        {
            if (!soundEffects_cue[3].IsPlaying)
            {
                soundEffects_cue[3] = soundBank.GetCue("UI_Click");
                soundEffects_cue[3].Play();
            }
        }

        public void PlayEngine(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[4].IsPrepared)
            {
                soundEffects_cue[4] = soundBank.GetCue("takeoff");
                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[4].Apply3D(listener, emitter);
                soundEffects_cue[4].Play();
            }
        }

        public void PlayMissileExplosion(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[5].IsPlaying)
            {
                soundEffects_cue[5] = soundBank.GetCue("explode");
                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[5].Apply3D(listener, emitter);
                soundEffects_cue[5].Play();
            }
        }

        public void PlayJetExplosion(Vector3 emitterPos, Vector3 listenerPos)
        {
            if (!soundEffects_cue[6].IsPlaying)
            {
                soundEffects_cue[6] = soundBank.GetCue("jet_explode");
                emitter.Position = emitterPos;
                listener.Position = listenerPos;

                soundEffects_cue[6].Apply3D(listener, emitter);
                soundEffects_cue[6].Play();
            }
        }

        public void PlayBG_01()
        {
            if (!music_cue[0].IsPlaying)
            {
                music_cue[0] = soundBank.GetCue("bg01");
                music_cue[0].Play();
            }
        }

        public void PlayBG_02()
        {
            if (!music_cue[1].IsPlaying)
            {
                music_cue[1] = soundBank.GetCue("bg02");
                music_cue[1].Play();
            }
        }

        public void PlayBG_03()
        {
            if (!music_cue[2].IsPlaying)
            {
                music_cue[2] = soundBank.GetCue("bg03");
                music_cue[2].Play();
            }
        }

        public void StopBG_01()
        {
            music_cue[0].Stop(AudioStopOptions.Immediate);
        }

        public void StopBG_02()
        {
            music_cue[1].Stop(AudioStopOptions.Immediate);
        }

        public void StopBG_03()
        {
            music_cue[2].Stop(AudioStopOptions.Immediate);
        }

        public void StopSFX()
        {
            for (int i = 0; i < soundEffects_cue.Length; i++)
            {
                if (soundEffects_cue[i].IsPlaying)
                    soundEffects_cue[i].Stop(AudioStopOptions.Immediate);
            }
        }

        public void StopMusic()
        {
            for (int i = 0; i < music_cue.Length; i++)
            {
                if (music_cue[i].IsPlaying)
                    music_cue[i].Stop(AudioStopOptions.Immediate);
            }
        }

        public void Pause()
        {
            for (int i = 0; i < soundEffects_cue.Length; i++)
            {
                if (soundEffects_cue[i].IsPlaying)
                    soundEffects_cue[i].Pause();
            }
        }

        public void Resume()
        {
            for (int i = 0; i < soundEffects_cue.Length; i++)
            {
                if (soundEffects_cue[i].IsPaused)
                    soundEffects_cue[i].Resume();
            }
        }

        public void Update()
        {
            audioEngine.Update();
        }
    }
}
