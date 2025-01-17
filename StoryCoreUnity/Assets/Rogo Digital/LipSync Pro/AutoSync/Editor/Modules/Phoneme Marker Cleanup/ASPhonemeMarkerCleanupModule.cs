﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogoDigital.Lipsync.AutoSync
{
	[AutoSyncModuleInfo("Phoneme Modification/Marker Cleanup Module", "Merges and re-times markers to create a simpler, smoother animation.", "Rogo Digital")]
	public class ASPhonemeMarkerCleanupModule : AutoSyncModule
	{
		public CleanupMode cleanupMode = CleanupMode.Legacy;
		public float cleanupAggression = 0.003f;
		public bool allowRetiming = true, allowMerging = true;
		public float maximumMarkerDensity = 0.5f;
		public float maximumGapForMerging = 0.03f;
		public float maximumRetimingError = 0.1f;

		public override ClipFeatures GetCompatibilityRequirements ()
		{
			return ClipFeatures.Phonemes;
		}

		public override ClipFeatures GetOutputCompatibility ()
		{
			return ClipFeatures.None;
		}

		public override void Process (LipSyncData inputClip, AutoSync.ASProcessDelegate callback)
		{
			List<PhonemeMarker> output = new List<PhonemeMarker>(inputClip.phonemeData);
			List<bool> markedForDeletion = new List<bool>();
			output.Sort(LipSync.SortTime);

			switch (cleanupMode)
			{
				default:
				case CleanupMode.Legacy:
					for (int m = 0; m < inputClip.phonemeData.Length; m++)
					{
						if (m > 0)
						{
							if (inputClip.phonemeData[m].time - inputClip.phonemeData[m - 1].time < cleanupAggression && !markedForDeletion[m - 1])
							{
								markedForDeletion.Add(true);
							}
							else
							{
								markedForDeletion.Add(false);
							}
						}
						else
						{
							markedForDeletion.Add(false);
						}
					}
					break;
				case CleanupMode.Simple:

					break;
				case CleanupMode.Advanced:

					break;
			}

			for (int m = 0; m < markedForDeletion.Count; m++)
			{
				if (markedForDeletion[m])
				{
					output.Remove(inputClip.phonemeData[m]);
				}
			}

			inputClip.phonemeData = output.ToArray();
			callback.Invoke(inputClip, new AutoSync.ASProcessDelegateData(true, "", ClipFeatures.None));
		}

		public enum CleanupMode
		{
			Legacy,
			Simple,
			Advanced,
		}
	}
}