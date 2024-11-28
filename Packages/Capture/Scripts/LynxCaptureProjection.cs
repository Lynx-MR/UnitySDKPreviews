using Lynx;
using System.Collections.Generic;
using UnityEngine;


namespace Lynx
{
    public static class LynxCaptureProjection
    {
        /// <summary>
        /// Generates 2 planes to be used as a basis for camera projection, each with uv and transform corresponding to the camera parameters
        /// </summary>
        /// <param name="material">Materials applied to both planes</param>
        /// <returns>Return 2 projection planes. LeftDisto as index 0, RightDisto as index 1</returns>
        public static GameObject[] SetupStereoUndistortMesh(Material material)
        {
            // Left camera
            LynxCaptureLibraryInterface.IntrinsicData intrinsicsLeft;
            LynxCaptureLibraryInterface.ExtrinsicData extrinsicsLeft;
            LynxCaptureLibraryInterface.ReadCameraParameters((byte)LynxCaptureAPI.ESensorType.RGB, 0, out intrinsicsLeft, out extrinsicsLeft);

            // Right camera
            LynxCaptureLibraryInterface.IntrinsicData intrinsicsRight;
            LynxCaptureLibraryInterface.ExtrinsicData extrinsicsRight;
            LynxCaptureLibraryInterface.ReadCameraParameters((byte)LynxCaptureAPI.ESensorType.RGB, 1, out intrinsicsRight, out extrinsicsRight);


            GameObject LeftDisto = new GameObject();
            LeftDisto.transform.parent = Camera.main.transform;
            LeftDisto.transform.localPosition = new Vector3((float)-extrinsicsLeft.position[0], (float)-extrinsicsLeft.position[2] - 0.11f, (float)extrinsicsLeft.position[1]);
            //Rotation is applyed after space convertion
            Vector3 convertedRotL = new Quaternion((float)extrinsicsLeft.orientation[0], (float)extrinsicsLeft.orientation[1], (float)extrinsicsLeft.orientation[2], (float)extrinsicsLeft.orientation[3]).eulerAngles;
            LeftDisto.transform.localRotation = Quaternion.Euler(new Vector3(180 - convertedRotL.z, 180 - convertedRotL.y, 0f));
            LeftDisto.name = "DistoLeft";
            MeshFilter filterL = LeftDisto.AddComponent<MeshFilter>();
            MeshRenderer rendL = LeftDisto.AddComponent<MeshRenderer>();
            Mesh distoMeshLeft = new Mesh();


            GameObject RightDisto = new GameObject();
            RightDisto.transform.parent = Camera.main.transform;
            LeftDisto.transform.localPosition = new Vector3((float)extrinsicsRight.position[0] + 0.05f, (float)-extrinsicsRight.position[2] - 0.09f, (float)extrinsicsRight.position[1]);
            Vector3 convertedRotR = new Quaternion((float)extrinsicsRight.orientation[0], (float)extrinsicsRight.orientation[1], (float)extrinsicsRight.orientation[2], (float)extrinsicsRight.orientation[3]).eulerAngles;
            LeftDisto.transform.localRotation = Quaternion.Euler(new Vector3(180 - convertedRotR.z, 180 - convertedRotR.y, 0f));
            RightDisto.name = "DistoRight";
            MeshFilter filterR = RightDisto.AddComponent<MeshFilter>();
            MeshRenderer rendR = RightDisto.AddComponent<MeshRenderer>();
            Mesh distoMeshRight = new Mesh();


            GenerateCameraMesh(intrinsicsLeft, true, out distoMeshLeft);
            GenerateCameraMesh(intrinsicsRight, false, out distoMeshRight);
            distoMeshLeft.name = "distoMeshLeft";
            distoMeshRight.name = "distoMeshRight";

            filterL.mesh = distoMeshLeft;
            filterR.mesh = distoMeshRight;

            rendL.material = material;
            rendR.material = material;

            return new GameObject[] { LeftDisto, RightDisto };
        }

        /// <summary>
        /// Generates 2 planes to be used as a basis for camera projection, each with uv and transform corresponding to the camera parameters
        /// </summary>
        /// <param name="LeftMaterial">Materials applied to the left plane</param>
        /// <param name="RightMaterial">Materials applied to the right plane</param>
        /// <returns>Return 2 projection planes. LeftDisto as index 0, RightDisto as index 1</returns>
        public static GameObject[] SetupStereoUndistortMesh(Material LeftMaterial, Material RightMaterial)
        {
            // Left camera
            LynxCaptureLibraryInterface.IntrinsicData intrinsicsLeft;
            LynxCaptureLibraryInterface.ExtrinsicData extrinsicsLeft;
            LynxCaptureLibraryInterface.ReadCameraParameters((byte)LynxCaptureAPI.ESensorType.RGB, 0, out intrinsicsLeft, out extrinsicsLeft);

            // Right camera
            LynxCaptureLibraryInterface.IntrinsicData intrinsicsRight;
            LynxCaptureLibraryInterface.ExtrinsicData extrinsicsRight;
            LynxCaptureLibraryInterface.ReadCameraParameters((byte)LynxCaptureAPI.ESensorType.RGB, 1, out intrinsicsRight, out extrinsicsRight);


            GameObject LeftDisto = new GameObject();
            LeftDisto.transform.parent = Camera.main.transform;
            
            LeftDisto.transform.localPosition = new Vector3((float)-extrinsicsLeft.position[0], (float)-extrinsicsLeft.position[2], (float)extrinsicsLeft.position[1]);
            //Rotation is applyed after space convertion
            Vector3 convertedRotL = new Quaternion((float)extrinsicsLeft.orientation[0], (float)extrinsicsLeft.orientation[1], (float)extrinsicsLeft.orientation[2], (float)extrinsicsLeft.orientation[3]).eulerAngles;
            LeftDisto.transform.localRotation = Quaternion.Euler(new Vector3(180 - convertedRotL.z, 180 - convertedRotL.y, 0f));
            LeftDisto.name = "DistoLeft";
            MeshFilter filterL = LeftDisto.AddComponent<MeshFilter>();
            MeshRenderer rendL = LeftDisto.AddComponent<MeshRenderer>();
            Mesh distoMeshLeft = new Mesh();


            GameObject RightDisto = new GameObject();
            RightDisto.transform.parent = Camera.main.transform;
            
            RightDisto.transform.localPosition = new Vector3((float)extrinsicsRight.position[0], (float)-extrinsicsRight.position[2], (float)extrinsicsRight.position[1]);
            Vector3 convertedRotR = new Quaternion((float)extrinsicsRight.orientation[0], (float)extrinsicsRight.orientation[1], (float)extrinsicsRight.orientation[2], (float)extrinsicsRight.orientation[3]).eulerAngles;
            RightDisto.transform.localRotation = Quaternion.Euler(new Vector3(180 - convertedRotR.z, 180 - convertedRotR.y, 0f));
            RightDisto.name = "DistoRight";
            MeshFilter filterR = RightDisto.AddComponent<MeshFilter>();
            MeshRenderer rendR = RightDisto.AddComponent<MeshRenderer>();
            Mesh distoMeshRight = new Mesh();


            GenerateCameraMesh(intrinsicsLeft, true, out distoMeshLeft);
            GenerateCameraMesh(intrinsicsRight, false, out distoMeshRight);
            distoMeshLeft.name = "distoMeshLeft";
            distoMeshRight.name = "distoMeshRight";

            filterL.mesh = distoMeshLeft;
            filterR.mesh = distoMeshRight;

            rendL.material = LeftMaterial;
            rendR.material = RightMaterial;


            return new GameObject[] { LeftDisto, RightDisto };
        }

        /// <summary>
        /// Generates a plane to be used as a basis for camera projection, with uv and transform corresponding to the camera parameters
        /// </summary>
        /// <param name="material">Material for display (need convertion from YUV to RGB</param>
        /// <param name="leftCameraFeed">should display left camera feed (true) or right (false)</param>
        /// <returns>Plane that is used for projection</returns>
        public static GameObject SetupMonoUndistortMesh(Material material, bool leftCameraFeed)
        {
            LynxCaptureLibraryInterface.IntrinsicData intrinsics;
            LynxCaptureLibraryInterface.ExtrinsicData extrinsics;
            if(leftCameraFeed)
                LynxCaptureLibraryInterface.ReadCameraParameters((byte)LynxCaptureAPI.ESensorType.RGB, 0, out intrinsics, out extrinsics);
            else
                LynxCaptureLibraryInterface.ReadCameraParameters((byte)LynxCaptureAPI.ESensorType.RGB, 1, out intrinsics, out extrinsics);


            GameObject Disto = new GameObject();
            Disto.transform.parent = Camera.main.transform;
            Disto.transform.localPosition = new Vector3((float)-extrinsics.position[0], (float)-extrinsics.position[2] - 0.11f, (float)extrinsics.position[1]);
            //Rotation is applyed after space convertion
            Vector3 convertedRotL = new Quaternion((float)extrinsics.orientation[0], (float)extrinsics.orientation[1], (float)extrinsics.orientation[2], (float)extrinsics.orientation[3]).eulerAngles;
            Disto.transform.localRotation = Quaternion.Euler(new Vector3(180 - convertedRotL.z, 180 - convertedRotL.y, 0f));
            Disto.name = "DistoMono";
            MeshFilter filter = Disto.AddComponent<MeshFilter>();
            MeshRenderer rend = Disto.AddComponent<MeshRenderer>();
            Mesh distoMesh = new Mesh();


            GenerateCameraMesh(intrinsics, leftCameraFeed, out distoMesh);
            distoMesh.name = "distoMeshMono";

            filter.mesh = distoMesh;

            rend.material = material;

            return Disto;
        }


        private static void UndistortFishEye4Params(LynxCaptureLibraryInterface.IntrinsicData pCameraInfo, double xi, double yi, out double xp, out double yp)
        {
            double accuracy = 1e-3;
            double xmin = 0.0;
            double ymin = 0.0;

            double xmax = 0.0;
            double ymax = 0.0;

            if ((xi - pCameraInfo.principalPoint[0]) > accuracy)
            {
                xmin = accuracy;
                xmax = 10.0;
            }
            else if ((xi - pCameraInfo.principalPoint[0]) < -accuracy)
            {
                xmin = -10.0;
                xmax = accuracy;
            }
            else
            {
                xmin = -accuracy;
                xmax = accuracy;
            }

            if ((yi - pCameraInfo.principalPoint[1]) > accuracy)
            {
                ymin = accuracy;
                ymax = 10.0;
            }
            else if ((yi - pCameraInfo.principalPoint[1]) < -accuracy)
            {
                ymin = -10.0;
                ymax = accuracy;
            }
            else
            {
                ymin = -accuracy;
                ymax = accuracy;
            }

            double xout = 0.0;
            double yout = 0.0;
            double x = 0.0;
            double y = 0.0;

            do
            {
                y = (ymin + ymax) / 2.0;
                DistortFisheye4Params(pCameraInfo, x, y, out xout, out yout);
                if (yout > yi)
                    ymax = y;
                else
                    ymin = y;
            } while (System.Math.Abs(yout - yi) > 1e-3);
            yp = y;

            do
            {
                x = (xmin + xmax) / 2.0;
                DistortFisheye4Params(pCameraInfo, x, y, out xout, out yout);
                if (xout > xi)
                    xmax = x;
                else
                    xmin = x;
            } while (System.Math.Abs(xout - xi) > 1e-3);
            xp = x;

        }

        private static void DistortFisheye4Params(LynxCaptureLibraryInterface.IntrinsicData pCameraInfo, double xp, double yp, out double xi, out double yi)
        {
            double r = System.Math.Sqrt(xp * xp + yp * yp);
            double teta = System.Math.Atan(r);
            double teta2 = teta * teta;
            double teta4 = teta2 * teta2;
            double teta6 = teta4 * teta2;
            double teta8 = teta4 * teta4;
            double tetad = teta * (1 + pCameraInfo.radialDistortion[0] * teta2 + pCameraInfo.radialDistortion[1] * teta4 + pCameraInfo.radialDistortion[2] * teta6 + pCameraInfo.radialDistortion[3] * teta8);

            double xd = tetad / r * xp;
            double yd = tetad / r * yp;

            double skew = 0.0;
            xi = pCameraInfo.focalLength[0] * (xd + skew * yd) + pCameraInfo.principalPoint[0];
            yi = pCameraInfo.focalLength[1] * yd + pCameraInfo.principalPoint[1];
        }

        private static void GenerateCameraMesh(LynxCaptureLibraryInterface.IntrinsicData pCameraInfo, bool isLeftCam, out Mesh mesh)
        {
            float imageWidth = 1536;
            float imageHeight = 1404;

            double xmin = 0.0;
            double xmax = 0.0;
            double ymin = 0.0;
            double ymax = 0.0;
            double tmp = 0.0f;


            Debug.Log("GenerateCameraMesh");
            UndistortFishEye4Params(pCameraInfo, 0.0, pCameraInfo.principalPoint[1], out xmin, out tmp);
            UndistortFishEye4Params(pCameraInfo, pCameraInfo.principalPoint[0], 0.0, out tmp, out ymin);
            UndistortFishEye4Params(pCameraInfo, imageWidth - 1.0, pCameraInfo.principalPoint[1], out xmax, out tmp);
            UndistortFishEye4Params(pCameraInfo, pCameraInfo.principalPoint[0], imageHeight - 1.0, out tmp, out ymax);
            int maxNum = 50;
            int numVerts = (maxNum + 1) * (maxNum + 1);

            List<Vector3> vertexPos = new List<Vector3>();
            List<Vector2> vertexUV = new List<Vector2>();
            List<int> trisIndex = new List<int>();
            List<Vector2> UVCamIndex = new List<Vector2>();

            // make vertexPos & UV
            for (int y = 0; y <= maxNum; ++y)
            {
                for (int x = 0; x <= maxNum; ++x)
                {
                    float fx = (float)xmin + ((float)x / (float)maxNum * ((float)xmax - (float)xmin));
                    float fy = (float)ymin + ((float)y / (float)maxNum * ((float)ymax - (float)ymin));

                    vertexPos.Add(new Vector3(fx, fy, 1.0f));

                    double u = 0, v = 0;
                    DistortFisheye4Params(pCameraInfo, fx, fy, out u, out v);
                    //normalize
                    u = u / imageWidth;
                    v = 1 - (v / imageHeight);
                    if (isLeftCam)
                        UVCamIndex.Add(new Vector2(0, 0));
                    else
                        UVCamIndex.Add(new Vector2(1, 0));

                    vertexUV.Add(new Vector2((float)u, (float)v));

                }
            }

            //make tris index
            int inner = 0;
            int a, b, c, d;
            int numPerRow = maxNum;

            for (int y = 0; y < maxNum; ++y)
            {
                for (int x = 0; x < maxNum; ++x)
                {
                    a = inner;
                    b = inner + 1;
                    c = b + (numPerRow + 1);
                    d = a + (numPerRow + 1);

                    trisIndex.Add(c);
                    trisIndex.Add(b);
                    trisIndex.Add(a);

                    trisIndex.Add(d);
                    trisIndex.Add(c);
                    trisIndex.Add(a);

                    inner++;
                }
                inner++;
            }

            //Apply mesh data to mesh output
            mesh = new Mesh();
            mesh.vertices = vertexPos.ToArray();
            mesh.uv = vertexUV.ToArray();
            mesh.uv2 = UVCamIndex.ToArray();
            mesh.triangles = trisIndex.ToArray();
        }


    }
}