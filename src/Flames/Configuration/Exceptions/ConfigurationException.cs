namespace Flames.Configuration.Exceptions;

public sealed class ConfigurationException(string message, Exception innerException) 
    : Exception(message, innerException) { }
