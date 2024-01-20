using System;
using System.IO;

internal class ConfigReader
{

    public string IPServerDB { get; private set; }
    public string BaseDeDatos { get; private set; }
    public string Usuario { get; private set; }
    public string Clave { get; private set; }
    public int MilisegundosTimer { get; private set; }

    public ConfigReader(string filePath)
    {
        try
        {
            //leemos las lineas de texto, recorremos el archivo en un array
            string[] lines = File.ReadAllLines(filePath);

            // Iteramos sobre cada línea en el archivo
            foreach (string line in lines)
            {
                //validamos que cada uno de los campos se separen por splits
                string[] parametros = line.Split('|');

                // Verificamos que haya al menos 5 partes en la línea
                if (parametros.Length >= 5)
                {
                    IPServerDB = parametros[0].Trim();
                    BaseDeDatos = parametros[1].Trim();
                    Usuario = parametros[2].Trim();
                    Clave = parametros[3].Trim();
                    MilisegundosTimer = int.Parse(parametros[4]);
                }
                else
                {
                    Console.WriteLine($"Error: La línea '{line}' no tiene el formato esperado.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo de configuración: {ex.Message}");
        }
    }
}
