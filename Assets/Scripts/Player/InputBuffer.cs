using UnityEngine;

public class InputBuffer<T> where T : struct
{
    private T? bufferedCommand;
    private float bufferTimer;
    private readonly float defaultWindow;

    public bool HasBufferedInput => bufferedCommand.HasValue;
    public T? Command => bufferedCommand;

    public InputBuffer(float defaultWindow = 0.15f)
    {
        this.defaultWindow = defaultWindow;
    }

    public void Buffer(T command)
    {
        bufferedCommand = command;
        bufferTimer = defaultWindow;
    }

    public T? Consume()
    {
        if (!bufferedCommand.HasValue)
        {
            return null;
        }

        T value = bufferedCommand.Value;
        Clear();
        return value;
    }

    public void Clear()
    {
        bufferedCommand = null;
        bufferTimer = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (bufferedCommand.HasValue)
        {
            bufferTimer -= deltaTime;
            if (bufferTimer <= 0f)
            {
                Clear();
            }
        }
    }
}
