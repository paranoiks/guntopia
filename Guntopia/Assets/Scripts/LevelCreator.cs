using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelCreator : MonoBehaviour {

    [SerializeField]
    private GameObject CubePrefab;

    private int[,] Grid;

    [SerializeField]
    private int LevelSize;

    [SerializeField]
    private int MinRoomSize;

    [SerializeField]
    private int MaxRoomSize;

    [SerializeField]
    private int RoomPlacementAttempts;

    private List<Rect> Rooms = new List<Rect>();

    private int[,] FourPermutations;

    private int RegionNumber = 1;

	// Use this for initialization
	void Start () {
        FourPermutations = new int[24, 4] {
            {1,2,3,4 }, {1,2,4,3 }, {1,3,2,4 }, {1,3,4,2 }, {1,4,2,3 }, {1,4,3,2 },
            {2,1,3,4 }, {2,1,4,3 }, {2,3,1,4 }, {2,3,4,1 }, {2,4,1,3 }, {2,4,3,1 },
            {3,1,2,4 }, {3,1,4,2 }, {3,2,1,4 }, {3,2,4,1 }, {3,4,1,2 }, {3,4,2,1 },
            {4,1,2,3 }, {4,1,3,2 }, {4,2,1,3 }, {4,2,3,1 }, {4,3,1,2 }, {4,3,2,1 } };            
	}

    private void CreateFrame()
    {
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                if (i == 0 || j == 0 || i == LevelSize - 1 || j == LevelSize - 1)
                {
                    Instantiate(CubePrefab, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
        }
    }

    private void CreateRooms()
    {
        for (int i = 0; i < RoomPlacementAttempts; i++)
        {
            int currentRoomWidth = Random.Range(MinRoomSize, MaxRoomSize);
            int currentRoomLength = Random.Range(MinRoomSize, MaxRoomSize);

            int currentRoomPositionX = Random.Range(currentRoomWidth, LevelSize - currentRoomWidth);
            int currentRoomPositionZ = Random.Range(currentRoomLength, LevelSize - currentRoomLength);

            Rect currentRoomRect = new Rect(currentRoomPositionX, currentRoomPositionZ, currentRoomWidth, currentRoomLength);

            bool overlap = false;
            //Debug.Log("Potential Room " + Rooms.Count);
            for (int r = 0; r < Rooms.Count; r++)
            {
                Rect room = Rooms[r];

                //Debug.Log("Checking room " + Rooms.Count + " (" + currentRoomRect.ToString() + ") against room " + r + " (" + Rooms[r].ToString() + ")");
                if (MathHelpers.RectanglesOverlap(currentRoomRect, room, 1))
                {
                    overlap = true;
                    break;
                }                
            }

            if (!overlap)
            {                
                //Debug.Log("ROOM " + Rooms.Count + ": " + currentRoomRect.ToString());
                Rooms.Add(currentRoomRect);                
            }
        }
    }

    private void CreateCorridors()
    {   
        Point startingPoint = new Point(0, 0);
        //FIND A STARTING POINT
        for (int i = 2; i < LevelSize - 2; i++)
        {
            for (int j = 2; j < LevelSize - 2; j++)
            {
                if (Grid[i, j] == 0)
                {
                    if (Grid[i + 1, j] == 0 && Grid[i - 1, j] == 0
                        && Grid[i, j + 1] == 0 && Grid[i, j - 1] == 0
                        && Grid[i + 1, j + 1] == 0 && Grid[i - 1, j + 1] == 0
                        && Grid[i + 1, j - 1] == 0 && Grid[i - 1, j - 1] == 0)
                    {
                        startingPoint = new Point(i, j);
                    }
                }
            }
        }

        StartFlood(startingPoint);
    }

    private void StartFlood(Point startingPoint)
    {
        Debug.Log("FLOOOOOD " + startingPoint.ToString());
        bool[,] processed = new bool[LevelSize, LevelSize];

        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                processed[i, j] = false;
            }
        }

        processed[(int)startingPoint.x, (int)startingPoint.y] = true;

        List<Point> queue = new List<Point>();
        queue.Add(startingPoint);
        Point previousNode = null;
        Dictionary<Point, Point> parents = new Dictionary<Point, Point>();

        while (queue.Count != 0)
        {
            int index = queue.Count - 1;
            Point currentNode = queue[index];
            queue.RemoveAt(index);
            //Debug.Log("CURRENT NODE " + currentNode.ToString());
            if (currentNode == startingPoint || CanFill(currentNode, parents, processed))
            {
                //Debug.Log("FLL " + currentNode.ToString());
                processed[currentNode.x, currentNode.y] = true;
                Grid[currentNode.x, currentNode.y] = RegionNumber;
                int p = Random.Range(0, 23);
                for (int i = 0; i < 4; i++)
                {
                    int number = FourPermutations[p, i];
                    switch (number)
                    {
                        case 1:
                            if (currentNode.x > 2 && !processed[currentNode.x - 1, currentNode.y])
                            {
                                Point nextNode = new Point(currentNode.x - 1, currentNode.y);
                                parents.Add(nextNode, currentNode);
                                queue.Add(nextNode);
                            }
                            break;
                        case 2:

                            if (currentNode.x < LevelSize - 3 && !processed[currentNode.x + 1, currentNode.y])
                            {
                                Point nextNode = new Point(currentNode.x + 1, currentNode.y);
                                parents.Add(nextNode, currentNode);
                                queue.Add(nextNode);
                            }
                            break;
                        case 3:

                            if (currentNode.y > 2 && !processed[currentNode.x, currentNode.y - 1])
                            {
                                Point nextNode = new Point(currentNode.x, currentNode.y - 1);
                                parents.Add(nextNode, currentNode);
                                queue.Add(nextNode);
                            }
                            break;
                        case 4:
                            if (currentNode.y < LevelSize - 3 && !processed[currentNode.x, currentNode.y + 1])
                            {
                                Point nextNode = new Point(currentNode.x, currentNode.y + 1);
                                parents.Add(nextNode, currentNode);
                                queue.Add(nextNode);
                            }
                            break;
                    }
                }
            }
            previousNode = currentNode;
        }
    }

    private bool CanFill(Point point, Dictionary<Point, Point> parents,  bool[,] processed)
    {        
        /*
        if (Grid[point.x, point.y] == 0
            && (Grid[point.x, point.y + 1] == 0 || (parents[point].x == point.x && parents[point].y == point.y + 1))
            && (Grid[point.x, point.y - 1] == 0 || (parents[point].x == point.x && parents[point].y == point.y - 1))
            && (Grid[point.x + 1, point.y] == 0 || (parents[point].x == point.x + 1 && parents[point].y == point.y))
            && (Grid[point.x + 1, point.y + 1] == 0 || (processed[point.x + 1, point.y + 1]))
            && (Grid[point.x + 1, point.y - 1] == 0 || (processed[point.x + 1, point.y - 1]))
            && (Grid[point.x - 1, point.y] == 0 || (parents[point].x == point.x - 1 && parents[point].y == point.y))
            && (Grid[point.x - 1, point.y + 1] == 0 || (processed[point.x - 1, point.y + 1]))
            && (Grid[point.x - 1, point.y - 1] == 0 || (processed[point.x - 1, point.y - 1])))
        {
            return true;
        }
        */
        
        if(Grid[point.x, point.y] != 0)
        {
            return false;
        }

        if((Grid[point.x, point.y + 1] != 0 && (parents[point].x != point.x || parents[point].y != point.y + 1))
            || (Grid[point.x, point.y - 1] != 0 && (parents[point].x != point.x || parents[point].y != point.y - 1))
            || (Grid[point.x + 1, point.y] != 0 && (parents[point].x != point.x + 1 || parents[point].y != point.y))
            || (Grid[point.x - 1, point.y] != 0 && (parents[point].x != point.x - 1 || parents[point].y != point.y)))
        {
            return false;
        }

        //++
        if(Grid[point.x+1, point.y+1] != 0 && !processed[point.x+1, point.y+1])
        {
            return false;
        }

        if (Grid[point.x + 1, point.y + 1] != 0 && processed[point.x + 1, point.y + 1])
        {
            if ((Grid[point.x + 1, point.y] == 0 || (Grid[point.x + 1, point.y] != 0 && !processed[point.x + 1, point.y]))
                && (Grid[point.x, point.y+1] == 0 || (Grid[point.x, point.y+1] != 0 && !processed[point.x, point.y+1])))
            {
                return false;
            }
        }

        //--
        if (Grid[point.x - 1, point.y - 1] != 0 && !processed[point.x - 1, point.y - 1])
        {
            return false;
        }

        if (Grid[point.x - 1, point.y - 1] != 0 && processed[point.x - 1, point.y - 1])
        {
            if ((Grid[point.x - 1, point.y] == 0 || (Grid[point.x - 1, point.y] != 0 && !processed[point.x - 1, point.y]))
                && (Grid[point.x, point.y - 1] == 0 || (Grid[point.x, point.y - 1] != 0 && !processed[point.x, point.y - 1])))
            {
                return false;
            }
        }


        //+-
        if (Grid[point.x + 1, point.y - 1] != 0 && !processed[point.x + 1, point.y - 1])
        {
            return false;
        }

        if (Grid[point.x + 1, point.y - 1] != 0 && processed[point.x + 1, point.y - 1])
        {
            if ((Grid[point.x + 1, point.y] == 0 || (Grid[point.x + 1, point.y] != 0 && !processed[point.x + 1, point.y]))
                && (Grid[point.x, point.y - 1] == 0 || (Grid[point.x, point.y - 1] != 0 && !processed[point.x, point.y - 1])))
            {
                return false;
            }
        }

        //-+
        if (Grid[point.x - 1, point.y + 1] != 0 && !processed[point.x - 1, point.y + 1])
        {
            return false;
        }

        if (Grid[point.x - 1, point.y + 1] != 0 && processed[point.x - 1, point.y + 1])
        {
            if ((Grid[point.x - 1, point.y] == 0 || (Grid[point.x - 1, point.y] != 0 && !processed[point.x - 1, point.y]))
                && (Grid[point.x, point.y + 1] == 0 || (Grid[point.x, point.y + 1] != 0 && !processed[point.x, point.y + 1])))
            {
                return false;
            }
        }

        return true;
    }

    private void PopulateGridWithRooms()
    {
        Grid = new int[LevelSize, LevelSize];

        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                Grid[i, j] = 0;
            }
        }

        foreach (var room in Rooms)
        {
            for (int i = (int)room.x; i < room.x + room.width; i++)
            {
                for (int j = (int)room.y; j < room.y + room.height; j++)
                {
                    Grid[i, j] = RegionNumber;
                }
            }
            RegionNumber++;
        }
    }

    private void InstantiateGrid()
    {
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {                
                if (Grid[i, j] == 1)
                {
                    Instantiate(CubePrefab, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
        }
    }

    private void ClearAll()
    {
        Debug.ClearDeveloperConsole();
        Rooms.Clear();
        foreach(var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            Destroy(cube);
        }
    }

    private void GenerateEverything()
    {
        ClearAll();
        CreateFrame();
        CreateRooms();
        PopulateGridWithRooms();
        CreateCorridors();
        InstantiateGrid();
    }
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.G))
        {
            GenerateEverything();
        }
	}
}
