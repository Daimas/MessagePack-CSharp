using MessagePack.Unity;

public interface IFormatterTemplate
{
    string Namespace { get; set; }

    ObjectSerializationInfo[] ObjectSerializationInfos { get; set; }

    string TransformText();
}