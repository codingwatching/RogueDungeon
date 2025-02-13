﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level.Generation.FloorDecorations.WaveFunctionCollapse
{
    public class FloorDecorationsGenerator : MonoBehaviour
    {
        [SerializeField] private List<FloorDecoration> floorDecos;
        [SerializeField] private FloorDecoration defaultDeco;
        [SerializeField] private CarpetCoordinateGenerator carpetCoordinateGenerator;
        
        public void Generate(GeneratedRoom generatedRoom)
        {
            List<Tuple<int,int>> carpetCoordinates = carpetCoordinateGenerator.Generate(generatedRoom);

            List<FloorDecoration> weightedDecos = new List<FloorDecoration>();
            for (var i = floorDecos.Count - 1; i >= 0; i--)
            {
                var floorDeco = floorDecos[i];
                for (var j = 0; j < floorDeco.rngWeighting; j++)
                {
                    weightedDecos.Add(floorDeco);
                }
            }
            
            List<List<Vector3>> floorLayout = generatedRoom.MapLayout;

            ////Wave Function Collapse prototype
            GameObject newObj = new GameObject("FloorDecorations");
            GameObject floorDecoParent = Instantiate(newObj, Vector3.zero, Quaternion.identity, generatedRoom.RoomParent.transform);
            Destroy(newObj);
            ApplyWfc(floorLayout, weightedDecos, floorDecoParent, carpetCoordinates);
        }

        private void ApplyWfc(List<List<Vector3>> floorLayout, List<FloorDecoration> weightedDecos,
            GameObject floorDecoParent, List<Tuple<int,int>> carpetCoordinates)
        {
             //1. Create uncollapsed WFC collection
            var wfcCollection = new List<List<WfcGrid>>();
            for (int z = 0; z < floorLayout.Count; z++)
            {
                wfcCollection.Add(new List<WfcGrid>());
                for (int x = 0; x < floorLayout[z].Count; x++)
                {
                    List<FloorDecoration> newWeightedList = new List<FloorDecoration>(weightedDecos);
                    List<FloorDecoration> newUnweightedList = new List<FloorDecoration>(floorDecos);
                    wfcCollection[z].Add(new WfcGrid(newWeightedList, newUnweightedList, defaultDeco));
                }
            }

            //2. Link WFC grids
            for (int z = 0; z < wfcCollection.Count; z++)
            {
                for (int x = 0; x < floorLayout[z].Count; x++)
                {
                    WfcGrid wfcGrid = wfcCollection[z][x];
                    if (z > 0)
                    {
                        WfcGrid downNeighbour = wfcCollection[z - 1][x];
                        wfcGrid.LinkDownNeighbour(downNeighbour);
                    }

                    if (z < wfcCollection.Count - 1)
                    {
                        WfcGrid upNeighbour = wfcCollection[z + 1][x];
                        wfcGrid.LinkUpNeighbour(upNeighbour);
                    }
                    
                    if (x > 0)
                    {
                        WfcGrid leftNeighbour = wfcCollection[z][x - 1];
                        wfcGrid.LinkLeftNeighbour(leftNeighbour);
                    }

                    if (x < wfcCollection[z].Count - 1)
                    {
                        WfcGrid rightNeighbour = wfcCollection[z][x + 1];
                        wfcGrid.LinkRightNeighbour(rightNeighbour);
                    }
                }
            }
            
            //2. Force grids with coordinates to generate anything on them
            for (int i = 0; i < carpetCoordinates.Count; i++)
            {
                Tuple<int,int> carpetCoordinate = carpetCoordinates[i];
                WfcGrid wfcGrid = wfcCollection[carpetCoordinate.Item1][carpetCoordinate.Item2];
                wfcGrid.ForceCollapseTo(FloorDecoName.Z_CARPET);
            }

            //3. Collapse a random grid randomly
            int randomZIndex = Random.Range(0, wfcCollection.Count);
            int randomXIndex = Random.Range(0, wfcCollection[0].Count);
            WfcGrid randomGrid = wfcCollection[randomZIndex][randomXIndex];
            randomGrid.RandomCollapse();

            //4. Iterate and Collapse
            IterateAndCollapse(wfcCollection);

            //5. Create decos based on WFC algo
            for (int z = 0; z < wfcCollection.Count; z++)
            {
                for (int x = 0; x < wfcCollection[z].Count; x++)
                {
                    GenerateDeco(wfcCollection[z][x].CollapsedValue, floorLayout[z][x], floorDecoParent);
                }
            }
        }

        private static void IterateAndCollapse(List<List<WfcGrid>> wfcCollection)
        {
            List<WfcGrid> flattenedGrid = wfcCollection.SelectMany(it => it).ToList();
            for (var i = 0; i < flattenedGrid.Count; i++)
            {
                flattenedGrid[i].Collapse();
            }

            if (flattenedGrid.Any(it => !it.IsCollapsed))
            {
                WfcGrid unCollapsedGrid = flattenedGrid.Find(it => !it.IsCollapsed);
                unCollapsedGrid.RandomCollapse();
                IterateAndCollapse(wfcCollection);
            }
        }
        
        private static void GenerateDeco(FloorDecoration wallDeco, Vector3 pos, GameObject floorDecoParent)
        {
            GameObject createdDeco = Instantiate(wallDeco.prefab, floorDecoParent.transform);
            createdDeco.transform.localPosition += pos;
            createdDeco.transform.Rotate(new Vector3(0, Random.Range(0, wallDeco.rotationRange), 0), Space.Self);
        }
    }
}