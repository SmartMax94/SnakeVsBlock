using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System;
using UnityEngine.UI;

/* This script deals wth exporting & importing data and non neural network related functions. */

public class ExternalFunctions : MonoBehaviour {
    
    //Reference to self
    public static ExternalFunctions core;

    //The path to which the weights will be exported
    public string exportPath;
    //The path from which the weights should be imported
    public string importPath;

    //Base name of the files to be exported.
    //Please note that multiple files will be exported (one for each snake). Each of these will have a base name and a few prefixes.
    public string exportFileName;

    //The base file name to be imported.
    //For example, if you have a folder with file Snake_1_0, Snake_1_1, Snake_1_2 etc., your base file name would be Snake_1, as the secondary prefixes are auto generated.
    public string importFileName;

    //Should files be overwritten? If this option is disabled, a prefix will be added to a file if its name collides with another.
    //For example, if Snake_0_* already exists, the system will auto generate a new prefix making the new batch of snakes named Snake_1_*.
    public bool overwrite;

    //The text object which will host the debug log.
    public Text debugText;

    //Should a new generation automatically be generated after the previous one is done?
    //If this option is set to false, the player will manually have to start each new generation.
    [HideInInspector]
    public bool autoStartNextGen;

    //Whenever this bool turned true, the next generation will be allowed to start.
    [HideInInspector]
    public bool startNextGen;

    //Auto Start Next gen object
    public GameObject autoStartNextGenObject;

    //Start next gen button
    public GameObject startNextGenObject;

    //Assigning reference
    void Awake () {
        if (core == null) {
            core = this;
        }
    }

    //On Start we simply add a listener to the start next gen button.
    void Start () {

        var b = startNextGenObject.GetComponent<Button>();
        b.onClick.RemoveAllListeners ();
        b.onClick.AddListener (delegate {startNextGen = true;});

        autoStartNextGenObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Auto Start Next Gen:Off";
    }

    //Toggling the autoStartNextGen boolean.
    public void toggleAutoGenStart () {

        autoStartNextGen = !autoStartNextGen;
        
        autoStartNextGenObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = autoStartNextGen ? "Auto Start Next Gen:On" : "Auto Start Next Gen:Off";

        if (autoStartNextGen && !startNextGen) {
            startNextGen = true;
        }

    }

    //This function allows debugging the latest log message, in-game. Please use this function instead of Debug.Log if you wish for your log messages to appear ingame.
    public void debug (object t) {
        var t2 = t.ToString();
        Debug.Log (t2);
        debugText.text += "     " + t2;
        debugText.text += "\n" + "     " + "---------------------" + "\n";
    }

    public void clearDebug () {
        debugText.text = "";
    }

    //This function is respensible for exporting and naming the snake weights.
    //It will essentially export all the weights of the current populations.
    public void export () {

        int prefix = 0;

        if (!overwrite) {

            while (System.IO.File.Exists (exportPath + "\\" + exportFileName + "_" + prefix.ToString() + "_0.xml")) {
                prefix += 1;
            }
            
        }
        
		try {
			
            int prefix2 = 0;

            foreach (Snake s in SnakeManager.core.p.snakes) {

                List<WeightMatrixExport> wmse = new List<WeightMatrixExport>();

                foreach (WeightMatrix wm in s.nn.wms) {
                    
                    WeightMatrixExport wme = new WeightMatrixExport ();

                    wme.matrix = new float [wm.rows * wm.columns];
                    var counter = 0;
                    for (int i1 = 0; i1 < wm.rows; i1++) {
                        for (int i2 = 0; i2 < wm.columns; i2++) {
                            wme.matrix [counter] = wm.matrix [i1, i2];
                            counter++;
                        }
                    }

                    wme.rows = wm.rows;
                    wme.columns = wm.columns;

                    wmse.Add (wme);
                    
                }

                WriteToXmlFile (exportPath + "\\" + exportFileName + "_" + prefix.ToString() + "_" + prefix2.ToString() + ".xml", wmse);

                prefix2++;
            }


		} catch (Exception e) {

			debug ("Could not export file. Error : " + e);
            
		} 

	}

    //This functions is responsible for importing existing weight files.
	public void import () {
		
		try {

			int prefix = 0;

            while (System.IO.File.Exists (importPath + "\\" + importFileName + "_"  + prefix.ToString() + ".xml")) {

                List<WeightMatrixExport> wmse = ReadFromXmlFile <List<WeightMatrixExport>> (importPath + "\\" + importFileName + "_" + prefix.ToString() + ".xml");
                
                for (int i = 0; i < wmse.Count; i++) {

                    WeightMatrix wm = new WeightMatrix (wmse[i].rows, wmse[i].columns);
                    var counter = 0;
                    for (int i1 = 0; i1 < wmse [i].rows; i1++) {
                        for (int i2 = 0; i2 < wmse [i].columns; i2++) {
                            wm.matrix [i1, i2] = wmse[i].matrix [counter];
                            counter++;
                        }
                    }

                    SnakeManager.core.p.snakes [prefix].nn.wms [i] = wm;
                }

                prefix++;
            }

			debug ("File loaded from : " + importPath);

		} catch (Exception e) {
			debug ("Could not import file. Error : " + e);
		} 
		
	}


    //This function is used to write xml files.
	public static void WriteToXmlFile <T> (string filePath, T objectToWrite, bool append = false) where T : new() {
		
		TextWriter writer = null;

		try {
			var serializer = new XmlSerializer(typeof(T));
			writer = new StreamWriter(filePath, append);
			serializer.Serialize(writer, objectToWrite);
		}

		finally {
			
			if (writer != null) {
				writer.Close();
			}
		}
	}

    //This function is used to read from xml files.
	public static T ReadFromXmlFile <T> (string filePath) where T : new() {
		
		TextReader reader = null;

		try {
			var serializer = new XmlSerializer(typeof(T));
			reader = new StreamReader(filePath);
			return (T)serializer.Deserialize(reader);
		}

		finally {
			
			if (reader != null) {
				reader.Close();
			}
			
		}
        
	}

}
