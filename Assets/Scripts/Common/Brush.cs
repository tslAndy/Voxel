using UnityEngine;
using Unity.Mathematics;
using System;
using Chunks;
using Helpers;
using UndoRedoSystem;
using UnityEngine.EventSystems;

namespace Common
{
    public class Brush : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Transform trs, cursorTrs;
        [SerializeField] private VoxelManager voxelManager;

        public event Action<int> onPicked;
        public BrushMode brushMode { get; set; }
        public byte matIndex { get; set; }

        private int3 _minBnd, _maxBnd;
        private long[] _changes;
        private CommandAccum _commAccum;

        private readonly string MainButton = "Fire1";

        private void Start()
        {
            _minBnd = new int3(0, 0, 0);
            _maxBnd = new int3(voxelManager.size, voxelManager.size, voxelManager.size);
            _changes = new long[voxelManager.size * voxelManager.size * voxelManager.size / 64];
        }

        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            switch (brushMode)
            {
                case BrushMode.Add:
                    AddState();
                    break;

                case BrushMode.Paint:
                    PaintState();
                    break;

                case BrushMode.Pick:
                    PickState();
                    break;

                case BrushMode.Del:
                    DelState();
                    break;

                default:
                    break;
            }
        }

        private void AddState()
        {
            if (Input.GetButtonUp(MainButton))
            {
                PushChanges();
                return;
            }

            int3 hitPos = GetHit(true);
            if (hitPos.x < 0)
                return;

            cursorTrs.position = new Vector3(hitPos.x + 0.5f, hitPos.y + 0.5f, hitPos.z + 0.5f);

            if (Input.GetButton(MainButton) && matIndex != 0) // && !this[hitPos]
            {
                AddChange(hitPos, voxelManager[hitPos], matIndex);
                voxelManager[hitPos] = matIndex;
            }
        }

        private void DelState()
        {
            if (Input.GetButtonUp(MainButton))
            {
                PushChanges();
                return;
            }

            int3 hitPos = GetHit();
            if (hitPos.x < 0)
                return;

            cursorTrs.position = new Vector3(hitPos.x + 0.5f, hitPos.y + 0.5f, hitPos.z + 0.5f);

            if (Input.GetButton(MainButton) && voxelManager[hitPos] != 0) // !this[hitPos]
            {
                AddChange(hitPos, voxelManager[hitPos], 0);
                voxelManager[hitPos] = 0;
            }
        }

        private void PaintState()
        {
            if (Input.GetButtonUp(MainButton))
            {
                PushChanges();
                return;
            }

            int3 hitPos = GetHit();
            if (hitPos.x < 0)
                return;

            cursorTrs.position = new Vector3(hitPos.x + 0.5f, hitPos.y + 0.5f, hitPos.z + 0.5f);

            if (Input.GetButton(MainButton) && voxelManager[hitPos] != 0) // !this[hitPos] && 
            {
                AddChange(hitPos, voxelManager[hitPos], matIndex);
                voxelManager[hitPos] = matIndex;
            }
        }

        private void PickState()
        {
            int3 hitPos = GetHit();
            if (hitPos.x < 0)
                return;

            cursorTrs.position = new Vector3(hitPos.x + 0.5f, hitPos.y + 0.5f, hitPos.z + 0.5f);

            if (Input.GetButton(MainButton) && voxelManager[hitPos] != 0)
            {
                matIndex = voxelManager[hitPos];
                onPicked?.Invoke(matIndex);
            }
        }

        private void AddChange(int3 pos, byte matIndexFrom, byte matIndexTo)
        {
            this[pos] = true;

            BrushComm comm = UndoRedoManager.Instance.GetCommand<BrushComm>();
            comm.pos = pos;
            comm.matIndexFrom = matIndexFrom;
            comm.matIndexTo = matIndexTo;

            _commAccum ??= UndoRedoManager.Instance.GetCommand<CommandAccum>();
            _commAccum.AddComm(comm);
        }

        private void PushChanges()
        {
            Array.Clear(_changes, 0, _changes.Length);

            if (_commAccum == null)
                return;

            UndoRedoManager.Instance.AddCommand(_commAccum);
            _commAccum = null;
        }

        private int3 GetHit(bool returnPrev = false)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            float3 dir = ray.direction;
            float3 rayOrigin = ray.origin;

            float3 invDir = new float3(1.0f / dir.x, 1.0f / dir.y, 1.0f / dir.z);

            float xCord;
            if (rayOrigin.x < _minBnd.x)
                xCord = 0;
            else if (rayOrigin.x > _maxBnd.x)
                xCord = _maxBnd.x;
            else
                xCord = invDir.x > 0 ? math.ceil(rayOrigin.x) : math.floor(rayOrigin.x);

            float tx = (xCord - rayOrigin.x) * invDir.x;
            float3 txHit = rayOrigin + dir * tx;
            bool txCheck = tx > 0 && _minBnd.y <= txHit.y && txHit.y < _maxBnd.y && _minBnd.z <= txHit.z && txHit.z < _maxBnd.z;
            if (!txCheck)
                tx = 1.0e10f;

            float yCord;
            if (rayOrigin.y < _minBnd.y)
                yCord = 0;
            else if (rayOrigin.y > _maxBnd.y)
                yCord = _maxBnd.y;
            else
                yCord = invDir.y > 0 ? math.ceil(rayOrigin.y) : math.floor(rayOrigin.y);

            float ty = (yCord - rayOrigin.y) * invDir.y;
            float3 tyHit = rayOrigin + dir * ty;
            bool tyCheck = ty > 0 && _minBnd.x <= tyHit.x && tyHit.x < _maxBnd.x && _minBnd.z <= tyHit.z && tyHit.z < _maxBnd.z;
            if (!tyCheck)
                ty = 1.0e10f;

            float zCord;
            if (rayOrigin.z < _minBnd.z)
                zCord = 0;
            else if (rayOrigin.z > _maxBnd.z)
                zCord = _maxBnd.z;
            else
                zCord = invDir.z > 0 ? math.ceil(rayOrigin.z) : math.floor(rayOrigin.z);

            float tz = (zCord - rayOrigin.z) * invDir.z;
            float3 tzHit = rayOrigin + dir * tz;
            bool tzCheck = tz > 0 && _minBnd.x <= tzHit.x && tzHit.x < _maxBnd.x && _minBnd.y <= tzHit.y && tzHit.y < _maxBnd.y;
            if (!tzCheck)
                tz = 1.0e10f;

            if (!(txCheck || tyCheck || tzCheck))
                return new int3(-1, -1, -1);

            float tHit = math.min(math.min(tx, ty), tz);
            float3 pos = rayOrigin + dir * (tHit + 0.0001f);
            float3 offset = math.sign(dir) * 0.5001f;

            int3 prevHit = new int3(-1, -1, -1);
            for (int i = 0; i < 1000; i++)
            {
                if (!(math.all(_minBnd <= pos) && math.all(pos < _maxBnd)))
                    break;

                int3 curHit = (int3)math.floor(pos);

                if (this[curHit])
                    return new int3(-1, -1, -1);

                if (voxelManager[curHit] != 0)
                    return returnPrev ? prevHit : curHit;

                prevHit = curHit;
                float3 tVec = (math.round(pos + offset) - pos) * invDir;
                float t = math.min(math.min(tVec.x, tVec.y), tVec.z);
                pos += dir * (t + 0.0001f);
            }

            return prevHit;
        }

        private bool this[int3 ind]
        {
            get
            {
                int index = MortonUtil.GetMorton(ind.x, ind.y, ind.z);
                return ((_changes[index / 64] >> (index % 64)) & 1) == 1;
            }
            set
            {
                int index = MortonUtil.GetMorton(ind.x, ind.y, ind.z);
                _changes[index / 64] &= ~(1L << (index % 64));
                _changes[index / 64] |= (value ? 1L : 0L) << (index % 64);
            }
        }

        public enum BrushMode
        {
            Add,
            Paint,
            Pick,
            Del
        }
    }
}