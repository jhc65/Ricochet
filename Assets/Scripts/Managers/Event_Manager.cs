/*
 Handles Cross Script communication through events. Personalized Event subscribing and creating.
 
 Written By: Alex Hollander
 Editted By: Amanda D. Barbadora
 */
using System.Collections.Generic;
using System;
using UnityEngine;

public class Event_Manager : MonoSingleton<Event_Manager>
{
    #region Variables

    /// <summary>
    /// The queue of events being processed. 
    ///     Think of a checkout line : events will be lined up and processed from the
    ///     front of the queue back.
    /// Looking specifically for iEvent interfaces.
    /// </summary>
    private Queue<iEvent> _eventQueue = new Queue<iEvent>();

    /// <summary>
    /// The amount of time the event manager
    /// will wait to process the next event.
    /// Limited to prevent stalls.
    /// </summary>
    [Range(0, 0.5f)]
    [SerializeField]
    private float queueWaitTime = 0.0f;

    [SerializeField]
    private bool waitTime;

    /// <summary>
    /// Format for listener functions ( delgates ).
    /// Example:
    ///     EventManager.instance.AddListener<ChatEvents.RequestChatLog>( OnRequestChatLog )
    ///     void OnRequestChatLog( ChatEvents.RequestChatLog @event ) { ... }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="@event"> The event that was passed through from the queue </param>
    public delegate void EventDelegate<T>(T @event) where T : iEvent;
    private delegate void EventDelegate(iEvent @event);

    /// <summary>
    /// A dictionary that stores the Event and the delegates associated with that event.
    /// </summary>
    private Dictionary<Type, EventDelegate> _delegates = new Dictionary<Type, EventDelegate>();

    private Dictionary<Delegate, EventDelegate> _delegateLookup = new Dictionary<Delegate, EventDelegate>();

    /// <summary>
    /// Temporary : used to add delgates to the dictionary above.
    /// </summary>
    private EventDelegate _delegate;

    private event EventDelegate Delegate
    {
        // Add / Adjust the value associated with the delegate being added to the dictionary
        add
        {
            _delegate -= value;
            _delegate += value;
        }
        // Remove the value associated with the delegate being added to the dictionary.
        remove
        {
            _delegate -= value;
        }
    }

    // --------------------------------- Tools For Delegate LookUp ---------------------------------------- //

    private Dictionary<Delegate, EventDelegate> _lookUp = new Dictionary<System.Delegate, EventDelegate>();


    /// <summary>
    /// Stores the scripts and the events they are listening too. Can now unsubscribe automatically through a single call.
    /// </summary>
    private Dictionary<System.Object, List<Delegate>> _objectLookUp = new Dictionary<object, List<System.Delegate>>();

    #endregion

    #region Methods
    #region HouseCleaning

    /// <summary>
    /// This function is called when the object this script is attached to
    /// is destroyed.
    ///     The Event Manager should stop all processes and clear the queue before
    ///     shutting down processes are run to prevent null reference errors and memory leaks.
    /// </summary>
    void OnDestroy( )
    {
        ClearAllDelegates();
        _eventQueue.Clear();
    }

    void ClearAllDelegates()
    {
        _delegates.Clear();
        _lookUp.Clear();
        _objectLookUp.Clear();
    }


    #endregion
    #region Listeners
    /// <summary>
    /// Add a listener to the dictionary. 
    /// </summary>
    /// <typeparam name="T"> The Event we are listening for </typeparam>
    /// <param name="@delegate"> The function called upon hearing the event </param>
    public void AddListener<T>(EventDelegate<T> @delegate) where T : iEvent
    {
        // Check if this object already has a spot in the dictionary to prevent 
        // duplicate key error
        if (_objectLookUp.ContainsKey(@delegate.Target))
        {
            // If this object is already registered, simply add the delegate to the key
            _objectLookUp[@delegate.Target].Add(@delegate);
        }
        else
        {
            // The Dictionary does not already contain the key, add the object as they key
            // and start the list of delegates associated with the object.
            _objectLookUp.Add(@delegate.Target, new List<Delegate> { @delegate });
        }

        RemoveListener(@delegate);
        AddDelegate<T>(@delegate);
    }

    /// <summary>
    /// Remove a listener from the dictionary.
    /// </summary>
    /// <typeparam name="T"> The Event </typeparam>
    /// <param name="@delegate"> The function called upon hearing the event </param>
    public void RemoveListener<T>( EventDelegate<T> @delegate) where T : iEvent
    {
        EventDelegate internalDel;

        // Check if this delegate does infact exist if it finds it, set
        // internalDel to the value it found.
        if( _lookUp.TryGetValue( @delegate, out internalDel ))
        {

            EventDelegate tempDel;   
            // If it exists, grab all of it's values and clear / remove them
            if( _delegates.TryGetValue( typeof(T), out tempDel))
            {
                tempDel -= internalDel;
                if( tempDel == null )
                {
                    // If internalDel was the only delegate
                    _delegates.Remove(typeof(T));
                }
                else
                {
                    // Set the value stored in _delegates to the one with the
                    // removed delegate.
                    _delegates[typeof(T)] = tempDel;
                }
                // Remove it from the look up.
                _lookUp.Remove(@delegate);
            }

        }
    }

    private EventDelegate AddDelegate<T>(EventDelegate<T> @delegate) where T : iEvent
    {
        if (_delegateLookup.ContainsKey(@delegate))
            return null;

        EventDelegate intDel = (@event) => @delegate((T)@event);
        _delegateLookup[@delegate] = intDel;

        if (_delegates.TryGetValue(typeof(T), out _delegate))
        {
            _delegates[typeof(T)] = _delegate += intDel;
        }
        else
        {
            _delegates[typeof(T)] = intDel;
        }
        return intDel;
    }

    #endregion
        #region Queue

    void Update()
    {
        // Used to keep track of the time between processing events
        float timer = 0.0f;

        while (_eventQueue.Count > 0)
        {
            iEvent @event = _eventQueue.Dequeue() as iEvent;
            if (timer > queueWaitTime && waitTime )
            {
                Debug.Log("EventManager tried to dequeue " + @event.ToString() + " stopped after : " + timer.ToString() + " seconds.");
                return;
            }

            // Inform all the listeners / delegates that the event has been
            // queued.
            ProcessEvent(@event);

            //Check time
            timer += Time.deltaTime;

        }

    }

    public bool QueueEvent( iEvent @event )
    {
        if( !_delegates.ContainsKey( @event.GetType()))
        {
            // If there are no listeners assigned to the event trying to be queued,
            // do not queue the event.
            return false;
        }

        _eventQueue.Enqueue(@event);
        return true;
    }

    /// <summary>
    /// Processes the events queued up.
    /// </summary>
    /// <param name="@event"></param>
    private void ProcessEvent( iEvent @event)
    {
        EventDelegate @delegate;

        // Double check that there are actually listeners / delegates for this event
        if( _delegates.TryGetValue(@event.GetType(), out @delegate ))
        {
            // If there was one, it's now stored in delegate and we can invoke the delegate
            // associated with this event we pass in.
            @delegate.Invoke(@event);
        }
        else
        {
            // There were no listeners.
            Debug.LogWarning("Event : " + @event.GetType() + " was found to have no listeners.");
        }
    }
    

    #endregion

    #endregion


}
