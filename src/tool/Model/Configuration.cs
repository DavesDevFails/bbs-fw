using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BBSFW.Model
{

	[XmlRoot("BBSFW", Namespace ="https://github.com/danielnilsson9/bbshd-fw")]
	public class Configuration
	{
		public const int CurrentVersion = 3;
		public const int MinVersion = 1;
		public const int MaxVersion = CurrentVersion;

		public const int ByteSizeV1 = 120;
		public const int ByteSizeV2 = 124;
		public const int ByteSizeV3 = 127;



		public static int GetByteSize(int version)
		{
			switch (version)
			{
				case 1:
					return ByteSizeV1;
				case 2:
					return ByteSizeV2;
				case 3:
					return ByteSizeV3;
			}

			return 0;
		}

		public enum AssistModeSelect
		{
			Off = 0,
			Standard = 1,
			Lights = 2,
			Pas0AndLights = 3
		}

		public enum AssistType
		{
			Disabled = 0x00,
			Pas = 0x01,
			Throttle = 0x02,
			PasAndThrottle = 0x03,
			VariablePas = 0x09,
			Cruise = 0x04
		};

		public enum TemperatureSensor
		{
			Disabled = 0x00,
			Controller = 0x01,
			Motor = 0x02,
			All = 0x03
		}

		public class AssistLevel
		{
			[XmlAttribute]
			public AssistType Type;

			[XmlAttribute]
			public uint MaxCurrentPercent;

			[XmlAttribute]
			public uint MaxThrottlePercent;

			[XmlAttribute]
			public uint MaxCadencePercent;

			[XmlAttribute]
			public uint MaxSpeedPercent;
		}

		// hmi
		[XmlIgnore]
		public bool UseFreedomUnits;

		// global
		public uint MaxCurrentAmps;
		public uint CurrentRampAmpsSecond;
		public float MaxBatteryVolts;
		public uint LowCutoffVolts;
		public uint MaxSpeedKph;

		// externals
		public bool UseSpeedSensor;
		public bool UseDisplay;
		public bool UsePushWalk;
		public bool UsePretension;
		public TemperatureSensor UseTemperatureSensor;

		// speed sensor
		public float WheelSizeInch;
		public uint NumWheelSensorSignals;
		
		// pas options
		public uint PasStartDelayPulses;
		public uint PasStopDelayMilliseconds;
		public uint PasKeepCurrentPercent;
		public uint PasKeepCurrentCadenceRpm;

		// throttle options
		public uint ThrottleStartMillivolts;
		public uint ThrottleEndMillivolts;
		public uint ThrottleStartPercent;

		// misc
		public bool ShowTemperatureOnPushWalk;

		// assists options
		public AssistModeSelect AssistModeSelection;
		public uint AssistStartupLevel;

		public AssistLevel[] StandardAssistLevels = new AssistLevel[10];
		public AssistLevel[] SportAssistLevels = new AssistLevel[10];


		public Configuration()
		{
			UseFreedomUnits = Properties.Settings.Default.UseFreedomUnits;
			MaxCurrentAmps = 0;
			CurrentRampAmpsSecond = 0;
			MaxBatteryVolts = 0;
			LowCutoffVolts = 0;

			UseSpeedSensor = false;
			UseDisplay = false;
			UsePushWalk = false;
			UseTemperatureSensor = TemperatureSensor.All;

			UsePretension = false;

			WheelSizeInch = 0;
			NumWheelSensorSignals = 0;
			MaxSpeedKph = 0;

			PasStartDelayPulses = 0;
			PasStopDelayMilliseconds = 0;
			PasKeepCurrentPercent = 0;
			PasKeepCurrentCadenceRpm = 0;

			ThrottleStartMillivolts = 0;
			ThrottleEndMillivolts = 0;
			ThrottleStartPercent = 0;


			ShowTemperatureOnPushWalk = false;

			AssistModeSelection = AssistModeSelect.Off;
			AssistStartupLevel = 0;

			for (int i = 0; i < StandardAssistLevels.Length; ++i)
			{
				StandardAssistLevels[i] = new AssistLevel();
			}

			for (int i = 0; i < SportAssistLevels.Length; ++i)
			{
				SportAssistLevels[i] = new AssistLevel();
			}
		}

		public bool ParseFromBufferV1(byte[] buffer)
		{
			if (buffer.Length != ByteSizeV1)
			{
				return false;
			}

			using (var s = new MemoryStream(buffer))
			{
				var br = new BinaryReader(s);

				UseFreedomUnits = br.ReadBoolean();

				MaxCurrentAmps = br.ReadByte();
				CurrentRampAmpsSecond = br.ReadByte();
				LowCutoffVolts = br.ReadByte();
				MaxSpeedKph = br.ReadByte();

				UseSpeedSensor = br.ReadBoolean();
				UseDisplay = br.ReadBoolean();
				UsePushWalk = br.ReadBoolean();

				WheelSizeInch = br.ReadUInt16() / 10f;
				NumWheelSensorSignals = br.ReadByte();

				PasStartDelayPulses = br.ReadByte();
				PasStopDelayMilliseconds = br.ReadByte() * 10u;

				ThrottleStartMillivolts = br.ReadUInt16();
				ThrottleEndMillivolts = br.ReadUInt16();
				ThrottleStartPercent = br.ReadByte();

				AssistModeSelection = (AssistModeSelect)br.ReadByte();
				AssistStartupLevel = br.ReadByte();

				for (int i = 0; i < 10; ++i)
				{
					StandardAssistLevels[i].Type = (AssistType)br.ReadByte();
					StandardAssistLevels[i].MaxCurrentPercent = br.ReadByte();
					StandardAssistLevels[i].MaxThrottlePercent = br.ReadByte();
					StandardAssistLevels[i].MaxCadencePercent = br.ReadByte();
					StandardAssistLevels[i].MaxSpeedPercent = br.ReadByte();
				}

				for (int i = 0; i < 10; ++i)
				{
					SportAssistLevels[i].Type = (AssistType)br.ReadByte();
					SportAssistLevels[i].MaxCurrentPercent = br.ReadByte();
					SportAssistLevels[i].MaxThrottlePercent = br.ReadByte();
					SportAssistLevels[i].MaxCadencePercent = br.ReadByte();
					SportAssistLevels[i].MaxSpeedPercent = br.ReadByte();
				}
			}

			// apply same default settings for non existing options in version
			MaxBatteryVolts = 0f;
			UseTemperatureSensor = TemperatureSensor.All;
			ShowTemperatureOnPushWalk = false;
			PasKeepCurrentPercent = 100;
			PasKeepCurrentCadenceRpm = 255;
			UsePretension = false;

			return true;
		}

		public bool ParseFromBufferV2(byte[] buffer)
		{
			if (buffer.Length != ByteSizeV2)
			{
				return false;
			}

			using (var s = new MemoryStream(buffer))
			{
				var br = new BinaryReader(s);

				UseFreedomUnits = br.ReadBoolean();

				MaxCurrentAmps = br.ReadByte();
				CurrentRampAmpsSecond = br.ReadByte();
				MaxBatteryVolts = br.ReadUInt16() / 100f;
				LowCutoffVolts = br.ReadByte();
				MaxSpeedKph = br.ReadByte();

				UseSpeedSensor = br.ReadBoolean();
				UseDisplay = br.ReadBoolean();
				UsePushWalk = br.ReadBoolean();
				UseTemperatureSensor = (TemperatureSensor)br.ReadByte();

				WheelSizeInch = br.ReadUInt16() / 10f;
				NumWheelSensorSignals = br.ReadByte();

				PasStartDelayPulses = br.ReadByte();
				PasStopDelayMilliseconds = br.ReadByte() * 10u;
				PasKeepCurrentCadenceRpm = 255;
				PasKeepCurrentPercent = 100;

				ThrottleStartMillivolts = br.ReadUInt16();
				ThrottleEndMillivolts = br.ReadUInt16();
				ThrottleStartPercent = br.ReadByte();

				ShowTemperatureOnPushWalk = br.ReadBoolean();

				AssistModeSelection = (AssistModeSelect)br.ReadByte();
				AssistStartupLevel = br.ReadByte();

				for (int i = 0; i < 10; ++i)
				{
					StandardAssistLevels[i].Type = (AssistType)br.ReadByte();
					StandardAssistLevels[i].MaxCurrentPercent = br.ReadByte();
					StandardAssistLevels[i].MaxThrottlePercent = br.ReadByte();
					StandardAssistLevels[i].MaxCadencePercent = br.ReadByte();
					StandardAssistLevels[i].MaxSpeedPercent = br.ReadByte();
				}

				for (int i = 0; i < 10; ++i)
				{
					SportAssistLevels[i].Type = (AssistType)br.ReadByte();
					SportAssistLevels[i].MaxCurrentPercent = br.ReadByte();
					SportAssistLevels[i].MaxThrottlePercent = br.ReadByte();
					SportAssistLevels[i].MaxCadencePercent = br.ReadByte();
					SportAssistLevels[i].MaxSpeedPercent = br.ReadByte();
				}
			}

			// apply same default settings for non existing options in version
			PasKeepCurrentPercent = 100;
			PasKeepCurrentCadenceRpm = 255;
			UsePretension = false;

			return true;
		}

		public bool ParseFromBufferV3(byte[] buffer)
		{
			if (buffer.Length != ByteSizeV3)
			{
				return false;
			}

			using (var s = new MemoryStream(buffer))
			{
				var br = new BinaryReader(s);

				UseFreedomUnits = br.ReadBoolean();

				MaxCurrentAmps = br.ReadByte();
				CurrentRampAmpsSecond = br.ReadByte();
				MaxBatteryVolts = br.ReadUInt16() / 100f;
				LowCutoffVolts = br.ReadByte();
				MaxSpeedKph = br.ReadByte();

				UseSpeedSensor = br.ReadBoolean();
				UseDisplay = br.ReadBoolean();
				UsePushWalk = br.ReadBoolean();
				UseTemperatureSensor = (TemperatureSensor)br.ReadByte();

				UsePretension = br.ReadBoolean();

				WheelSizeInch = br.ReadUInt16() / 10f;
				NumWheelSensorSignals = br.ReadByte();

				PasStartDelayPulses = br.ReadByte();
				PasStopDelayMilliseconds = br.ReadByte() * 10u;
				PasKeepCurrentPercent = br.ReadByte();
				PasKeepCurrentCadenceRpm = br.ReadByte();

				ThrottleStartMillivolts = br.ReadUInt16();
				ThrottleEndMillivolts = br.ReadUInt16();
				ThrottleStartPercent = br.ReadByte();

				ShowTemperatureOnPushWalk = br.ReadBoolean();

				AssistModeSelection = (AssistModeSelect)br.ReadByte();
				AssistStartupLevel = br.ReadByte();

				for (int i = 0; i < 10; ++i)
				{
					StandardAssistLevels[i].Type = (AssistType)br.ReadByte();
					StandardAssistLevels[i].MaxCurrentPercent = br.ReadByte();
					StandardAssistLevels[i].MaxThrottlePercent = br.ReadByte();
					StandardAssistLevels[i].MaxCadencePercent = br.ReadByte();
					StandardAssistLevels[i].MaxSpeedPercent = br.ReadByte();
				}

				for (int i = 0; i < 10; ++i)
				{
					SportAssistLevels[i].Type = (AssistType)br.ReadByte();
					SportAssistLevels[i].MaxCurrentPercent = br.ReadByte();
					SportAssistLevels[i].MaxThrottlePercent = br.ReadByte();
					SportAssistLevels[i].MaxCadencePercent = br.ReadByte();
					SportAssistLevels[i].MaxSpeedPercent = br.ReadByte();
				}
			}

			return true;
		}

		public byte[] WriteToBuffer()
		{
			using (var s = new MemoryStream())
			{
				var bw = new BinaryWriter(s);

				bw.Write(UseFreedomUnits);

				bw.Write((byte)MaxCurrentAmps);
				bw.Write((byte)CurrentRampAmpsSecond);
				bw.Write((UInt16)(MaxBatteryVolts * 100));
				bw.Write((byte)LowCutoffVolts);
				bw.Write((byte)MaxSpeedKph);

				bw.Write(UseSpeedSensor);
				bw.Write(UseDisplay);
				bw.Write(UsePushWalk);
				bw.Write((byte)UseTemperatureSensor);

				bw.Write(UsePretension);
					
				bw.Write((UInt16)(WheelSizeInch * 10));
				bw.Write((byte)NumWheelSensorSignals);

				bw.Write((byte)PasStartDelayPulses);
				bw.Write((byte)(PasStopDelayMilliseconds / 10u));
				bw.Write((byte)PasKeepCurrentPercent);
				bw.Write((byte)PasKeepCurrentCadenceRpm);

				bw.Write((UInt16)ThrottleStartMillivolts);
				bw.Write((UInt16)ThrottleEndMillivolts);
				bw.Write((byte)ThrottleStartPercent);

				bw.Write(ShowTemperatureOnPushWalk);

				bw.Write((byte)AssistModeSelection);
				bw.Write((byte)AssistStartupLevel);

				for (int i = 0; i < 10; ++i)
				{
					bw.Write((byte)StandardAssistLevels[i].Type);
					bw.Write((byte)StandardAssistLevels[i].MaxCurrentPercent);
					bw.Write((byte)StandardAssistLevels[i].MaxThrottlePercent);
					bw.Write((byte)StandardAssistLevels[i].MaxCadencePercent);
					bw.Write((byte)StandardAssistLevels[i].MaxSpeedPercent);
				}

				for (int i = 0; i < 10; ++i)
				{
					bw.Write((byte)SportAssistLevels[i].Type);
					bw.Write((byte)SportAssistLevels[i].MaxCurrentPercent);
					bw.Write((byte)SportAssistLevels[i].MaxThrottlePercent);
					bw.Write((byte)SportAssistLevels[i].MaxCadencePercent);
					bw.Write((byte)SportAssistLevels[i].MaxSpeedPercent);
				}

				return s.ToArray();
			}
		}

		public void CopyFrom(Configuration cfg)
		{
			UseFreedomUnits = cfg.UseFreedomUnits;
			MaxCurrentAmps = cfg.MaxCurrentAmps;
			CurrentRampAmpsSecond = cfg.CurrentRampAmpsSecond;
			MaxBatteryVolts = cfg.MaxBatteryVolts;
			LowCutoffVolts = cfg.LowCutoffVolts;
			UseSpeedSensor = cfg.UseSpeedSensor;
			UseDisplay = cfg.UseDisplay;
			UsePushWalk = cfg.UsePushWalk;
			UsePretension = cfg.UsePretension;
			UseTemperatureSensor = cfg.UseTemperatureSensor;
			WheelSizeInch = cfg.WheelSizeInch;
			NumWheelSensorSignals = cfg.NumWheelSensorSignals;
			MaxSpeedKph = cfg.MaxSpeedKph;
			PasStartDelayPulses = cfg.PasStartDelayPulses;
			PasStopDelayMilliseconds = cfg.PasStopDelayMilliseconds;
			PasKeepCurrentPercent = cfg.PasKeepCurrentPercent;
			PasKeepCurrentCadenceRpm = cfg.PasKeepCurrentCadenceRpm;
			ThrottleStartMillivolts = cfg.ThrottleStartMillivolts;
			ThrottleEndMillivolts = cfg.ThrottleEndMillivolts;
			ThrottleStartPercent = cfg.ThrottleStartPercent;
			ShowTemperatureOnPushWalk = cfg.ShowTemperatureOnPushWalk;
			AssistModeSelection = cfg.AssistModeSelection;
			AssistStartupLevel = cfg.AssistStartupLevel;

			for (int i = 0; i < Math.Min(cfg.StandardAssistLevels.Length, 10); ++i)
			{
				StandardAssistLevels[i].Type = cfg.StandardAssistLevels[i].Type;
				StandardAssistLevels[i].MaxCurrentPercent = cfg.StandardAssistLevels[i].MaxCurrentPercent;
				StandardAssistLevels[i].MaxThrottlePercent = cfg.StandardAssistLevels[i].MaxThrottlePercent;
				StandardAssistLevels[i].MaxCadencePercent = cfg.StandardAssistLevels[i].MaxCadencePercent;
				StandardAssistLevels[i].MaxSpeedPercent = cfg.StandardAssistLevels[i].MaxSpeedPercent;
			}

			for (int i = 0; i < Math.Min(cfg.SportAssistLevels.Length, 10); ++i)
			{
				SportAssistLevels[i].Type = cfg.SportAssistLevels[i].Type;
				SportAssistLevels[i].MaxCurrentPercent = cfg.SportAssistLevels[i].MaxCurrentPercent;
				SportAssistLevels[i].MaxThrottlePercent = cfg.SportAssistLevels[i].MaxThrottlePercent;
				SportAssistLevels[i].MaxCadencePercent = cfg.SportAssistLevels[i].MaxCadencePercent;
				SportAssistLevels[i].MaxSpeedPercent = cfg.SportAssistLevels[i].MaxSpeedPercent;
			}
		}

		public void ReadFromFile(string filepath)
		{
			var serializer = new XmlSerializer(typeof(Configuration));

			using (var reader = new FileStream(filepath, FileMode.Open))
			{
				var obj = serializer.Deserialize(reader) as Configuration;
				CopyFrom(obj);
			}
		}

		public void WriteToFile(string filepath)
		{
			var serializer = new XmlSerializer(typeof(Configuration));
			var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
			using (var xmlWriter = XmlWriter.Create(new StreamWriter(filepath), settings))
			{
				serializer.Serialize(xmlWriter, this);
			}
		}


		public void Validate()
		{
			ValidateLimits(MaxCurrentAmps, 5, 33, "Max Current (A)");
			ValidateLimits(CurrentRampAmpsSecond, 1, 255, "Current Ramp (A/s)");
			ValidateLimits((uint)MaxBatteryVolts, 1, 100, "Max Battery Voltage (V)");
			ValidateLimits(LowCutoffVolts, 1, 100, "Low Voltage Cut Off (V)");

			ValidateLimits((uint)WheelSizeInch, 10, 40, "Wheel Size (inch)");
			ValidateLimits(NumWheelSensorSignals, 1, 10, "Wheel Sensor Signals");
			ValidateLimits(MaxSpeedKph, 0, 100, "Max Speed (km/h)");

			ValidateLimits(PasStartDelayPulses, 0, 24, "Pas Delay (pulses)");
			ValidateLimits(PasStopDelayMilliseconds, 50, 1000, "Pas Stop Delay (ms)");
			ValidateLimits(PasKeepCurrentPercent, 10, 100, "Pas Keep Current (%)");
			ValidateLimits(PasKeepCurrentCadenceRpm, 0, 255, "Pas Keep Current Cadence (rpm)");

			ValidateLimits(ThrottleStartMillivolts, 200, 2500, "Throttle Start (mV)");
			ValidateLimits(ThrottleEndMillivolts, 2500, 5000, "Throttle End (mV)");
			ValidateLimits(ThrottleStartPercent, 0, 100, "Throttle Start (%)");

			ValidateLimits(AssistStartupLevel, 0, 9, "Assist Startup Level");
		}


		private void ValidateLimits(uint value, uint min, uint max, string name)
		{
			if (value < min || value > max)
			{
				throw new Exception(name + " must be in interval " + min + "-" + max + ".");
			}
		}

	}
}
