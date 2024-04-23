using OBSStudioClient;
using OBSStudioClient.Classes;
using OBSStudioClient.Enums;
using OBSStudioClient.Events;
using OBSStudioClient.Messages;
using OBSStudioClient.Responses;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Text.Json;
using Monitor = OBSStudioClient.Classes.Monitor;

namespace SunsUpStreamsUp.Facades;

/*
 * How to generate this facade:
 * 1. Copy the third-party source code for decompiled ObsClient.cs to a temporary file in this project
 * 2. Extract an interface from ObsClient, which creates this IObsClient interface
 * 3. Add the <inheritdoc>, [GeneratedCode], INotifyPropertyChanged, and IDisposable to this interface
 * 4. Delete the temporary ObsClient.cs file
 * 5. Create a ObsClientFacade subclass of ObsClient which implements the new IObsClient interface
 * 6. Register ObsClientFacade in dependency injection context with IObsClient interface
 * 7. Inject IObjsClient into dependent classes
 *
 * Note: in step 2, make sure you're extracting the interface from ObsClient instead of its ObsClientFacade subclass, otherwise documentation comments will not be copied to the interface.
 *
 * If you want to tell which methods changed between versions, Telerik JustAssembly (free) is useful: https://www.telerik.com/justassembly
 */

/// <inheritdoc cref="ObsClient"/>
[GeneratedCode("OBSClient", "2.1.1")]
public interface IObsClient: INotifyPropertyChanged, IDisposable {

    /// <summary>
    /// Gets or sets the maximum amount of time, in milliseconds, the <see cref="T:OBSStudioClient.ObsClient" /> to wait for an OBS Studio response after making a request.
    /// </summary>
    /// <remarks>
    /// The minimum value is 150. Please take into account that when sending Batch Requests, specifically with long Sleep requests, this default value of 500 might not be enough.
    /// Should a response not be received in time, an Exception will be thrown.
    /// </remarks>
    int RequestTimeout { get; set; }

    /// <summary>
    /// Gets the current state of the connection to OBS Studio.
    /// </summary>
    /// <remarks>
    /// You should only call ConnectAsync when this state is <see cref="F:OBSStudioClient.Enums.ConnectionState.Disconnected" />.
    /// </remarks>
    ConnectionState ConnectionState { get; }

    /// <summary>
    /// Gets or sets the value that indicates whether the <see cref="T:OBSStudioClient.ObsClient" /> should automatically try to reconnect to OBS Studio.
    /// </summary>
    /// <remarks>
    /// When the value is True, the client will automatically try to reconnect to OBS Studio when:
    /// - The connection was closed by OBS Studio
    /// - You were kicked
    /// - OBS Studio closed
    /// - You sent an invalid message or your password was incorrect.
    /// When you call <see cref="M:OBSStudioClient.ObsClient.Disconnect" />, this setting is automatically set to False."/&gt;
    /// Setting it back to True, after calling <see cref="M:OBSStudioClient.ObsClient.Disconnect" />, will automatically try to reconnect.
    /// </remarks>
    bool AutoReconnect { get; set; }

    /// <summary>Gets the number of bytes sent to OBS Studio.</summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    long TotalBytesSent { get; }

    /// <summary>
    /// Gets the total number of bytes received from OBS Studio throughout the lifetime of the <see cref="T:OBSStudioClient.ObsClient" />.
    /// </summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    long TotalBytesReceived { get; }

    /// <summary>
    /// Gets the total number of messages sent to OBS Studio throughout the lifetime of the <see cref="T:OBSStudioClient.ObsClient" />.
    /// </summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    int TotalMessagesSent { get; }

    /// <summary>
    /// Gets the total number of messages received from OBS Studio throughout the lifetime of the <see cref="T:OBSStudioClient.ObsClient" />.
    /// </summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    int TotalMessagesReceived { get; }

    /// <summary>Gets the number of bytes sent to OBS Studio.</summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    long SessionBytesSent { get; }

    /// <summary>
    /// Gets the number of bytes received from OBS Studio in the current session.
    /// </summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    long SessionBytesReceived { get; }

    /// <summary>
    /// Gets the number of messages sent to OBS Studio in the current session.
    /// </summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    int SessionMessagesSent { get; }

    /// <summary>
    /// Gets the number of messages received from OBS Studio in the current session.
    /// </summary>
    /// <remarks>
    /// You will not be notified through the PropertyChanged event when this value changes.
    /// </remarks>
    int SessionMessagesReceived { get; }

    /// <summary>
    /// Gets or sets the maximum number of times the <see cref="T:OBSStudioClient.ObsClient" /> should retry a request when OBS Studio is not ready to perform the request.
    /// </summary>
    /// <remarks>
    /// This typically occurs when OBS Studio is not ready to perform the request, e.g. when it is still starting up. It will authenticate the client, but not yet accept requests.
    /// </remarks>
    int MaxRequestRetries { get; set; }

    /// <summary>
    /// Gets or sets the interval, in milliseconds, the <see cref="T:OBSStudioClient.ObsClient" /> should wait before retrying a request when OBS Studio is not ready to perform the request.
    /// </summary>
    int RequestRetryInterval { get; set; }

    /// <summary>
    /// Asynchronous event, triggered when the connection with OBS Studio is closed.
    /// </summary>
    event EventHandler<ConnectionClosedEventArgs>? ConnectionClosed;

    /// <summary>
    /// Opens the connection to OBS Studio and tries to authenticate the session.
    /// </summary>
    /// <param name="autoReconnect">Value to indicate whether the client should automatically try to reconnect to OBS Studio.</param>
    /// <param name="password">The OBS Studio WebSockets password or empty to connect without authentication. Defaults to empty.</param>
    /// <param name="hostname">The hostname of the computer running OBS Studio to connect to. Defaults to "localhost".</param>
    /// <param name="port">The Port on which the OBS Studio WebSocket interface is listenting. Default to 4455.</param>
    /// <param name="eventSubscription">The events to subscribe to. Defaults to All events.</param>
    /// <returns>True, when the connection was succesfully established, and False otherwise.</returns>
    /// <remarks>
    /// When True is returned, this does not mean that authentication has succeeded. Authentication will be handled asynchronously.
    /// You can use the <see cref="E:OBSStudioClient.ObsClient.PropertyChanged" /> event to see whether the <see cref="P:OBSStudioClient.ObsClient.ConnectionState" /> is Connected, which indicates succesfull authenticaiton.
    /// When the client is already connected, disconnect first.
    /// </remarks>
    Task<bool> ConnectAsync(bool autoReconnect = false, string password = "", string hostname = "localhost", int port = 4455, EventSubscriptions eventSubscription = EventSubscriptions.All);

    /// <summary>Closes the connection to OBS Studio.</summary>
    void Disconnect();

    /// <summary>Sends a Batch Request.</summary>
    /// <param name="requestBatchMessage">The <see cref="T:OBSStudioClient.Messages.RequestBatchMessage" /> to send.</param>
    /// <param name="timeOutInMilliseconds">The timeout for the request. (Defauls to <see cref="P:OBSStudioClient.ObsClient.RequestTimeout" />.)</param>
    /// <returns>The responses for the individual requests.</returns>
    /// <remarks>Since batch requests typically take more time than individual request, you have the opportunity here to override the default timeout for this specific call.</remarks>
    Task<RequestResponseMessage[]> SendRequestBatchAsync(RequestBatchMessage requestBatchMessage, int? timeOutInMilliseconds = null);

    /// <summary>
    /// Sends a Reidentify request to OBS Studio, typically to subscribe to a different set of events.
    /// </summary>
    /// <param name="eventSubscription">The events to subscribe to.</param>
    /// <returns>An awaitable task.</returns>
    Task ReidentifyAsync(EventSubscriptions eventSubscription);

    /// <summary>
    /// Gets the value of a "slot" from the selected persistent data realm.
    /// </summary>
    /// <param name="realm">The data realm to select. OBS_WEBSOCKET_DATA_REALM_GLOBAL or OBS_WEBSOCKET_DATA_REALM_PROFILE</param>
    /// <param name="slotName">The name of the slot to retrieve data from</param>
    /// <returns>Value associated with the slot. null if not set</returns>
    Task<object?> GetPersistentData(Realm realm, string slotName);

    /// <summary>
    /// Sets the value of a "slot" from the selected persistent data realm.
    /// </summary>
    /// <param name="realm">The data realm to select. OBS_WEBSOCKET_DATA_REALM_GLOBAL or OBS_WEBSOCKET_DATA_REALM_PROFILE</param>
    /// <param name="slotName">The name of the slot to retrieve data from</param>
    /// <param name="slotValue">The value to apply to the slot</param>
    Task SetPersistentData(Realm realm, string slotName, object slotValue);

    /// <summary>Gets an array of all scene collections</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SceneCollectionListResponse" /></returns>
    Task<SceneCollectionListResponse> GetSceneCollectionList();

    /// <summary>Switches to a scene collection.</summary>
    /// <param name="sceneCollectionName">The name of the current scene collection</param>
    /// <remarks>
    /// Note: This will block until the collection has finished changing.
    /// </remarks>
    Task SetCurrentSceneCollection(string sceneCollectionName);

    /// <summary>
    /// Creates a new scene collection, switching to it in the process.
    /// </summary>
    /// <param name="sceneCollectionName">Name for the new scene collection</param>
    /// <remarks>
    /// Note: This will block until the collection has finished changing.
    /// </remarks>
    Task CreateSceneCollection(string sceneCollectionName);

    /// <summary>Gets an array of all profiles</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.ProfileListResponse" /></returns>
    Task<ProfileListResponse> GetProfileList();

    /// <summary>Switches to a profile.</summary>
    /// <param name="profileName">Name of the profile to switch to</param>
    Task SetCurrentProfile(string profileName);

    /// <summary>Creates a new profile, switching to it in the process</summary>
    /// <param name="profileName">Name for the new profile</param>
    Task CreateProfile(string profileName);

    /// <summary>
    /// Removes a profile. If the current profile is chosen, it will change to a different profile first.
    /// </summary>
    /// <param name="profileName">Name of the profile to remove</param>
    Task RemoveProfile(string profileName);

    /// <summary>
    /// Gets a parameter from the current profile's configuration.
    /// </summary>
    /// <param name="parameterCategory">Category of the parameter to get</param>
    /// <param name="parameterName">Name of the parameter to get</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.ProfileParameterResponse" /></returns>
    Task<ProfileParameterResponse> GetProfileParameter(string parameterCategory, string parameterName);

    /// <summary>
    /// Sets the value of a parameter in the current profile's configuration.
    /// </summary>
    /// <param name="parameterCategory">Category of the parameter to set</param>
    /// <param name="parameterName">Name of the parameter to set</param>
    /// <param name="parameterValue">Value of the parameter to set. Use null to delete</param>
    Task SetProfileParameter(string parameterCategory, string parameterName, string? parameterValue);

    /// <summary>Gets the current video settings.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.VideoSettingsResponse" /></returns>
    Task<VideoSettingsResponse> GetVideoSettings();

    /// <summary>Sets the current video settings.</summary>
    /// <param name="fpsNumerator">Numerator of the fractional FPS value.</param>
    /// <param name="fpsDenominator">Denominator of the fractional FPS value.</param>
    /// <param name="baseWidth">Width of the base (canvas) resolution in pixels.</param>
    /// <param name="baseHeight">Height of the base (canvas) resolution in pixels.</param>
    /// <param name="outputWidth">Width of the output resolution in pixels.</param>
    /// <param name="outputHeight">Height of the output resolution in pixels.</param>
    Task SetVideoSettings(float? fpsNumerator, float? fpsDenominator, int? baseWidth, int? baseHeight, int? outputWidth, int? outputHeight);

    /// <summary>
    /// Gets the current stream service settings (stream destination).
    /// </summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.StreamServiceSettingsResponse" /></returns>
    Task<StreamServiceSettingsResponse> GetStreamServiceSettings();

    /// <summary>
    /// Sets the current stream service settings (stream destination).
    /// </summary>
    /// <param name="streamServiceType">Type of stream service to apply. Example: rtmp_common or rtmp_custom</param>
    /// <param name="streamServiceSettings">Settings to apply to the service</param>
    /// <remarks>
    /// Note: Simple RTMP settings can be set with type rtmp_custom and the settings fields server and key.
    /// </remarks>
    Task SetStreamServiceSettings(string streamServiceType, object streamServiceSettings);

    /// <summary>
    /// Gets the current directory that the record output is set to.
    /// </summary>
    /// <returns>Output directory</returns>
    Task<string> GetRecordDirectory();

    /// <summary>Occurs when OBS has begun the shutdown process.</summary>
    event EventHandler? ExitStarted;

    /// <summary>Occurs when an event has been emitted from a vendor.</summary>
    /// <remarks>
    /// A vendor is a unique name registered by a third-party plugin or script, which allows for custom requests and events to be added to obs-websocket. If a plugin or script implements vendor requests or events, documentation is expected to be provided with them.
    /// </remarks>
    event EventHandler<VendorEventEventArgs>? VendorEvent;

    /// <summary>
    /// Occurs when a custom event emitted by <see cref="M:OBSStudioClient.ObsClient.BroadcastCustomEvent(System.Text.Json.JsonElement)" />.
    /// </summary>
    event EventHandler<CustomEventEventArgs>? CustomEvent;

    /// <summary>
    /// Occurs when the current scene collection has begun changing.
    /// </summary>
    /// <remarks>
    /// Note: We recommend using this event to trigger a pause of all polling requests, as performing any requests during a scene collection change is considered undefined behavior and can cause crashes!
    /// </remarks>
    event EventHandler<SceneCollectionNameEventArgs>? CurrentSceneCollectionChanging;

    /// <summary>Occurs when the current scene collection has changed.</summary>
    /// <remarks>
    /// Note: If polling has been paused during CurrentSceneCollectionChanging, this is the que to restart polling.
    /// </remarks>
    event EventHandler<SceneCollectionNameEventArgs>? CurrentSceneCollectionChanged;

    /// <summary>Occurs when the scene collection list has changed.</summary>
    event EventHandler<SceneCollectionListEventArgs>? SceneCollectionListChanged;

    /// <summary>Occurs when the current profile has begun changing.</summary>
    event EventHandler<ProfileNameEventArgs>? CurrentProfileChanging;

    /// <summary>Occurs when the current profile has changed.</summary>
    event EventHandler<ProfileNameEventArgs>? CurrentProfileChanged;

    /// <summary>Occurs when the profile list has changed.</summary>
    event EventHandler<ProfileListEventArgs>? ProfileListChanged;

    /// <summary>Occurs when a new scene has been created.</summary>
    event EventHandler<SceneModifiedEventArgs>? SceneCreated;

    /// <summary>Occurs when a scene has been removed.</summary>
    event EventHandler<SceneModifiedEventArgs>? SceneRemoved;

    /// <summary>Occurs when the name of a scene has changed.</summary>
    event EventHandler<SceneNameChangedEventArgs>? SceneNameChanged;

    /// <summary>Occurs when the current program scene has changed.</summary>
    event EventHandler<SceneNameEventArgs>? CurrentProgramSceneChanged;

    /// <summary>Occurs when the current preview scene has changed.</summary>
    event EventHandler<SceneNameEventArgs>? CurrentPreviewSceneChanged;

    /// <summary>Occurs when the list of scenes has changed.</summary>
    event EventHandler<SceneListEventArgs>? SceneListChanged;

    /// <summary>Occurs when an input has been created.</summary>
    event EventHandler<InputCreatedEventArgs>? InputCreated;

    /// <summary>Occurs when an input has been removed.</summary>
    event EventHandler<InputNameEventArgs>? InputRemoved;

    /// <summary>Occurs when the name of an input has changed.</summary>
    event EventHandler<InputNameChangedEventArgs>? InputNameChanged;

    /// <summary>Occurs when an input's active state has changed.</summary>
    /// <remarks>
    /// When an input is active, it means it's being shown by the program feed.
    /// </remarks>
    event EventHandler<InputActiveStateChangedEventArgs>? InputActiveStateChanged;

    /// <summary>Occurs when an input's show state has changed.</summary>
    /// <remarks>
    /// When an input is showing, it means it's being shown by the preview or a dialog.
    /// </remarks>
    event EventHandler<InputShowStateChangedEventArgs>? InputShowStateChanged;

    /// <summary>Occurs when an input's mute state has changed.</summary>
    event EventHandler<InputMuteStateChangedEventArgs>? InputMuteStateChanged;

    /// <summary>Occurs when an input's volume level has changed.</summary>
    event EventHandler<InputVolumeChangedEventArgs>? InputVolumeChanged;

    /// <summary>
    /// Occurs when the audio balance value of an input has changed.
    /// </summary>
    event EventHandler<InputAudioBalanceChangedEventArgs>? InputAudioBalanceChanged;

    /// <summary>Occurs when the sync offset of an input has changed.</summary>
    event EventHandler<InputAudioSyncOffsetChangedEventArgs>? InputAudioSyncOffsetChanged;

    /// <summary>
    /// Occurs when the audio tracks of an input have changed.
    /// </summary>
    event EventHandler<InputAudioTracksChangedEventArgs>? InputAudioTracksChanged;

    /// <summary>Occurs when the monitor type of an input has changed.</summary>
    event EventHandler<InputAudioMonitorTypeChangedEventArgs>? InputAudioMonitorTypeChanged;

    /// <summary>
    /// Occurs every 50 milliseconds providing volume levels of all active inputs.
    /// </summary>
    event EventHandler<InputVolumeMetersEventArgs>? InputVolumeMeters;

    /// <summary>Occurs when the current scene transition has changed.</summary>
    event EventHandler<TransitionNameEventArgs>? CurrentSceneTransitionChanged;

    /// <summary>
    /// Occurs when the current scene transition duration has changed.
    /// </summary>
    event EventHandler<TransitionDurationEventArgs>? CurrentSceneTransitionDurationChanged;

    /// <summary>Occurs when a scene transition has started.</summary>
    event EventHandler<TransitionNameEventArgs>? SceneTransitionStarted;

    /// <summary>Occurs when a scene transition has completed fully.</summary>
    /// <remarks>
    /// Note: Does not appear to trigger when the transition is interrupted by the user.
    /// </remarks>
    event EventHandler<TransitionNameEventArgs>? SceneTransitionEnded;

    /// <summary>
    /// Occurs when a scene transition's video has completed fully.
    /// </summary>
    /// <remarks>
    /// Useful for stinger transitions to tell when the video actually ends. SceneTransitionEnded only signifies the cut point, not the completion of transition playback.
    /// Note: Appears to be called by every transition, regardless of relevance.
    /// </remarks>
    event EventHandler<TransitionNameEventArgs>? SceneTransitionVideoEnded;

    /// <summary>
    /// Occurs when a source's filter list has been reindexed.
    /// </summary>
    event EventHandler<SourceFiltersEventArgs>? SourceFilterListReindexed;

    /// <summary>Occurs when a filter has been added to a source.</summary>
    event EventHandler<SourceFilterCreatedEventArgs>? SourceFilterCreated;

    /// <summary>Occurs when a filter has been removed from a source.</summary>
    event EventHandler<SourceFilterRemovedEventArgs>? SourceFilterRemoved;

    /// <summary>Occurs when the name of a source filter has changed.</summary>
    event EventHandler<SourceFilterNameChangedEventArgs>? SourceFilterNameChanged;

    /// <summary>
    /// Occurs when a source filter's enable state has changed.
    /// </summary>
    event EventHandler<SourceFilterEnableStateChangedEventArgs>? SourceFilterEnableStateChanged;

    /// <summary>Occurs when a scene item has been created.</summary>
    event EventHandler<SceneItemCreatedEventArgs>? SceneItemCreated;

    /// <summary>Occurs when a scene item has been removed.</summary>
    event EventHandler<SceneItemRemovedEventArgs>? SceneItemRemoved;

    /// <summary>Occurs when a scene's item list has been reindexed.</summary>
    event EventHandler<SceneItemListReindexedEventArgs>? SceneItemListReindexed;

    /// <summary>Occurs when a scene item's enable state has changed.</summary>
    event EventHandler<SceneItemEnableStateChangedEventArgs>? SceneItemEnableStateChanged;

    /// <summary>Occurs when a scene item's lock state has changed.</summary>
    event EventHandler<SceneItemLockStateChangedEventArgs>? SceneItemLockStateChanged;

    /// <summary>Occurs when a scene item has been selected in the Ui.</summary>
    event EventHandler<SceneItemSelectedEventArgs>? SceneItemSelected;

    /// <summary>
    /// Occurs when the transform/crop of a scene item has changed.
    /// </summary>
    event EventHandler<SceneItemTransformChangedEventArgs>? SceneItemTransformChanged;

    /// <summary>
    /// Occurs when the state of the stream output has changed.
    /// </summary>
    event EventHandler<OutputStateChangedEventArgs>? StreamStateChanged;

    /// <summary>
    /// Occurs when the state of the record output has changed.
    /// </summary>
    event EventHandler<RecordStateChangedEventArgs>? RecordStateChanged;

    /// <summary>
    /// Occurs when the state of the replay buffer output has changed.
    /// </summary>
    event EventHandler<OutputStateChangedEventArgs>? ReplayBufferStateChanged;

    /// <summary>
    /// Occurs when the state of the virtualcam output has changed.
    /// </summary>
    event EventHandler<OutputStateChangedEventArgs>? VirtualcamStateChanged;

    /// <summary>Occurs when the replay buffer has been saved.</summary>
    event EventHandler<ReplayBufferSavedEventArgs>? ReplayBufferSaved;

    /// <summary>Occurs when a media input has started playing.</summary>
    event EventHandler<InputNameEventArgs>? MediaInputPlaybackStarted;

    /// <summary>Occurs when a media input has finished playing.</summary>
    event EventHandler<InputNameEventArgs>? MediaInputPlaybackEnded;

    /// <summary>Occurs when an action has been performed on an input.</summary>
    event EventHandler<MediaInputActionTriggeredEventArgs>? MediaInputActionTriggered;

    /// <summary>Occurs when Studio mode has been enabled or disabled.</summary>
    event EventHandler<StudioModeStateChangedEventArgs>? StudioModeStateChanged;

    /// <summary>Occurs when a screenshot has been saved.</summary>
    /// <remarks>
    /// Note: Triggered for the screenshot feature available in Settings -&gt; Hotkeys -&gt; Screenshot Output ONLY. Applications using Get/SaveSourceScreenshot should implement a CustomEvent if this kind of inter-client communication is desired.
    /// </remarks>
    event EventHandler<ScreenshotSavedEventArgs>? ScreenshotSaved;

    /// <summary>Gets an array of all of a source's filters.</summary>
    /// <param name="sourceName">Name of the source</param>
    /// <returns>Array of <see cref="T:OBSStudioClient.Classes.Filter" /></returns>
    Task<Filter[]> GetSourceFilterList(string sourceName);

    /// <summary>Gets the default settings for a filter kind.</summary>
    /// <param name="filterKind">Filter kind to get the default settings for</param>
    /// <returns>Object of default settings for the filter kind</returns>
    Task<Dictionary<string, object>> GetSourceFilterDefaultSettings(string filterKind);

    /// <summary>
    /// Creates a new filter, adding it to the specified source.
    /// </summary>
    /// <param name="sourceName">Name of the source to add the filter to</param>
    /// <param name="filterName">Name of the new filter to be created</param>
    /// <param name="filterKind">The kind of filter to be created</param>
    /// <param name="filterSettings">Settings object to initialize the filter with</param>
    Task CreateSourceFilter(string sourceName, string filterName, string filterKind, Dictionary<string, object>? filterSettings);

    /// <summary>Removes a filter from a source.</summary>
    /// <param name="sourceName">Name of the source the filter is on</param>
    /// <param name="filterName">Name of the filter to remove</param>
    Task RemoveSourceFilter(string sourceName, string filterName);

    /// <summary>Sets the name of a source filter (rename).</summary>
    /// <param name="sourceName">Name of the source the filter is on</param>
    /// <param name="filterName">Current name of the filter</param>
    /// <param name="newFilterName">New name for the filter</param>
    Task SetSourceFilterName(string sourceName, string filterName, string newFilterName);

    /// <summary>Gets the info for a specific source filter.</summary>
    /// <param name="sourceName">Name of the source</param>
    /// <param name="filterName">Name of the filter</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SourceFilterResponse" /></returns>
    Task<SourceFilterResponse> GetSourceFilter(string sourceName, string filterName);

    /// <summary>Sets the index position of a filter on a source.</summary>
    /// <param name="sourceName">Name of the source the filter is on</param>
    /// <param name="filterName">Name of the filter</param>
    /// <param name="filterIndex">New index position of the filter (&gt;= 0)</param>
    Task SetSourceFilterIndex(string sourceName, string filterName, int filterIndex);

    /// <summary>Sets the settings of a source filter.</summary>
    /// <param name="sourceName">Name of the source the filter is on</param>
    /// <param name="filterName">Name of the filter to set the settings of</param>
    /// <param name="filterSettings">Object of settings to apply</param>
    /// <param name="overlay">True == apply the settings on top of existing ones, False == reset the input to its defaults, then apply settings.</param>
    Task SetSourceFilterSettings(string sourceName, string filterName, Dictionary<string, object> filterSettings, bool overlay = true);

    /// <summary>Sets the enable state of a source filter.</summary>
    /// <param name="sourceName">Name of the source the filter is on</param>
    /// <param name="filterName">Name of the filter</param>
    /// <param name="filterEnabled">New enable state of the filter</param>
    Task SetSourceFilterEnabled(string sourceName, string filterName, bool filterEnabled);

    /// <summary>Gets data about the current plugin and RPC version.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.VersionResponse" /> object with OBS Studio Version information.</returns>
    Task<VersionResponse> GetVersion();

    /// <summary>
    /// Gets statistics about OBS, obs-websocket, and the current session.
    /// </summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.StatsResponse" /> object with OBS Studio Statistics.</returns>
    Task<StatsResponse> GetStats();

    /// <summary>
    /// Broadcasts a CustomEvent to all WebSocket clients. Receivers are clients which are identified and subscribed.
    /// </summary>
    /// <param name="eventData">Data payload to emit to all receivers</param>
    Task BroadcastCustomEvent(JsonElement eventData);

    /// <summary>Call a request registered to a vendor.</summary>
    /// <param name="vendorName">Name of the vendor to use</param>
    /// <param name="requestType">The request type to call</param>
    /// <param name="requestData">Object containing appropriate request data</param>
    Task<CallVendorResponse> CallVendorRequest(string vendorName, string requestType, JsonElement? requestData);

    /// <summary>Gets an array of all hotkey names in OBS</summary>
    /// <returns>Array of hotkey names</returns>
    Task<string[]> GetHotkeyList();

    /// <summary>
    /// Triggers a hotkey using its name. See <see cref="M:OBSStudioClient.ObsClient.GetHotkeyList" />
    /// </summary>
    /// <param name="hotkeyName">Name of the hotkey to trigger</param>
    Task TriggerHotkeyByName(string hotkeyName);

    /// <summary>Triggers a hotkey using a sequence of keys.</summary>
    /// <param name="keyId">The OBS key ID to use. See https://github.com/obsproject/obs-studio/blob/master/libobs/obs-hotkeys.h</param>
    /// <param name="keyModifier">Key modifiers to apply</param>
    Task TriggerHotkeyByKeySequence(ObsKey? keyId, KeyModifier? keyModifier);

    /// <summary>Gets an array of all inputs in OBS.</summary>
    /// <param name="inputKind">Restrict the array to only inputs of the specified kind</param>
    /// <returns>An array of <see cref="T:OBSStudioClient.Messages.Input" /></returns>
    Task<Input[]> GetInputList(string? inputKind = null);

    /// <summary>Gets an array of all available input kinds in OBS.</summary>
    /// <param name="unversioned">True == Return all kinds as unversioned, False == Return with version suffixes (if available)</param>
    /// <returns>Array of input kinds</returns>
    Task<string[]> GetInputKindList(bool unversioned = false);

    /// <summary>Gets the names of all special inputs.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SpecialInputsResponse" /></returns>
    Task<SpecialInputsResponse> GetSpecialInputs();

    /// <summary>
    /// Creates a new input, adding it as a scene item to the specified scene.
    /// </summary>
    /// <param name="sceneName">Name of the scene to add the input to as a scene item</param>
    /// <param name="inputName">Name of the new input to created</param>
    /// <param name="inputKind">The kind of input to be created</param>
    /// <param name="inputSettings">Settings object to initialize the input with</param>
    /// <param name="sceneItemEnabled">Whether to set the created scene item to enabled or disabled</param>
    /// <returns>ID of the newly created scene item</returns>
    Task<int> CreateInput(string sceneName, string inputName, string inputKind, Input? inputSettings, bool sceneItemEnabled = true);

    /// <summary>Removes an existing input.</summary>
    /// <param name="inputName">Name of the input to remove</param>
    /// <remarks>
    /// Note: Will immediately remove all associated scene items.
    /// </remarks>
    Task RemoveInput(string inputName);

    /// <summary>Sets the name of an input (rename).</summary>
    /// <param name="inputName">Current input name</param>
    /// <param name="newInputName">New name for the input</param>
    Task SetInputName(string inputName, string newInputName);

    /// <summary>Gets the default settings for an input kind.</summary>
    /// <param name="inputKind">Input kind to get the default settings for</param>
    /// <returns>Default settings for the input kind.</returns>
    Task<Dictionary<string, object>> GetInputDefaultSettings(string inputKind);

    /// <summary>Gets the settings of an input.</summary>
    /// <param name="inputName">Name of the input to get the settings of</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.InputSettingsResponse" /></returns>
    /// <remarks>
    /// Note: Does not include defaults. To create the entire settings object, overlay inputSettings over the defaultInputSettings provided by GetInputDefaultSettings.
    /// </remarks>
    Task<InputSettingsResponse> GetInputSettings(string inputName);

    /// <summary>Sets the settings of an input.</summary>
    /// <param name="inputName">Name of the input to set the settings of</param>
    /// <param name="inputSettings">Object of settings to apply</param>
    /// <param name="overlay">True == apply the settings on top of existing ones, False == reset the input to its defaults, then apply settings.</param>
    Task SetInputSettings(string inputName, Dictionary<string, object> inputSettings, bool overlay = true);

    /// <summary>Gets the audio mute state of an input.</summary>
    /// <param name="inputName">Name of input to get the mute state of</param>
    /// <returns>Whether the input is muted</returns>
    Task<bool> GetInputMute(string inputName);

    /// <summary>Sets the audio mute state of an input.</summary>
    /// <param name="inputName">Name of the input to set the mute state of</param>
    /// <param name="inputMuted">Whether to mute the input or not</param>
    Task SetInputMute(string inputName, bool inputMuted);

    /// <summary>Toggles the audio mute state of an input.</summary>
    /// <param name="inputName">Name of the input to toggle the mute state of</param>
    /// <returns>Whether the input has been muted or unmuted</returns>
    Task<bool> ToggleInputMute(string inputName);

    /// <summary>Gets the current volume setting of an input.</summary>
    /// <param name="inputName">Name of the input to get the volume of</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.InputVolumeResponse" /></returns>
    Task<InputVolumeResponse> GetInputVolume(string inputName);

    /// <summary>Sets the volume setting of an input.</summary>
    /// <param name="inputName">Name of the input to set the volume of</param>
    /// <param name="inputVolumeMul">Volume setting in mul.</param>
    Task SetInputVolumeMul(string inputName, float inputVolumeMul);

    /// <summary>Sets the volume setting of an input.</summary>
    /// <param name="inputName">Name of the input to set the volume of</param>
    /// <param name="inputVolumeDb">Volume setting in dB.</param>
    Task SetInputVolumeDb(string inputName, float inputVolumeDb);

    /// <summary>Gets the audio balance of an input.</summary>
    /// <param name="inputName">Name of the input to get the audio balance of</param>
    /// <returns>Audio balance value from 0.0-1.0</returns>
    Task<float> GetInputAudioBalance(string inputName);

    /// <summary>Sets the audio balance of an input.</summary>
    /// <param name="inputName">Name of the input to set the audio balance of</param>
    /// <param name="inputAudioBalance">New audio balance value.</param>
    Task SetInputAudioBalance(string inputName, float inputAudioBalance);

    /// <summary>Gets the audio sync offset of an input.</summary>
    /// <param name="inputName">Name of the input to get the audio sync offset of</param>
    /// <returns>Audio sync offset in milliseconds</returns>
    /// <remarks>Note: The audio sync offset can be negative too!</remarks>
    Task<int> GetInputAudioSyncOffset(string inputName);

    /// <summary>Sets the audio sync offset of an input.</summary>
    /// <param name="inputName">Name of the input to set the audio sync offset of</param>
    /// <param name="inputAudioSyncOffset">New audio sync offset in milliseconds.</param>
    Task SetInputAudioSyncOffset(string inputName, int inputAudioSyncOffset);

    /// <summary>Gets the audio monitor type of an input.</summary>
    /// <param name="inputName">Name of the input to get the audio monitor type of</param>
    /// <returns>Audio monitor type</returns>
    Task<MonitorType> GetInputAudioMonitorType(string inputName);

    /// <summary>Sets the audio monitor type of an input.</summary>
    /// <param name="inputName">Name of the input to set the audio monitor type of</param>
    /// <param name="monitorType">Audio monitor type</param>
    Task SetInputAudioMonitorType(string inputName, MonitorType monitorType);

    /// <summary>
    /// Gets the enable state of all audio tracks of an input.
    /// </summary>
    /// <param name="inputName">Name of the input</param>
    /// <returns>Object of audio tracks and associated enable states</returns>
    Task<AudioTracks> GetInputAudioTracks(string inputName);

    /// <summary>Sets the enable state of audio tracks of an input.</summary>
    /// <param name="inputName">Name of the input</param>
    /// <param name="inputAudioTracks">Track settings to apply</param>
    Task SetInputAudioTracks(string inputName, AudioTracks inputAudioTracks);

    /// <summary>
    /// Gets the items of a list property from an input's properties.
    /// </summary>
    /// <param name="inputName">Name of the input</param>
    /// <param name="propertyName">Name of the list property to get the items of</param>
    /// <returns>Array of items in the list property</returns>
    /// <remarks>
    /// Note: Use this in cases where an input provides a dynamic, selectable list of items. For example, display capture, where it provides a list of available displays.
    /// </remarks>
    Task<PropertyItem[]> GetInputPropertiesListPropertyItems(string inputName, string propertyName);

    /// <summary>Presses a button in the properties of an input.</summary>
    /// <param name="inputName">Name of the input</param>
    /// <param name="propertyName">Name of the button property to press</param>
    /// <remarks>
    /// Note: Use this in cases where there is a button in the properties of an input that cannot be accessed in any other way. For example, browser sources, where there is a refresh button.
    /// </remarks>
    Task PressInputPropertiesButton(string inputName, string propertyName);

    /// <summary>Gets the status of a media input.</summary>
    /// <param name="inputName">Name of the media input</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.MediaInputStatusResponse" /></returns>
    Task<MediaInputStatusResponse> GetMediaInputStatus(string inputName);

    /// <summary>Sets the cursor position of a media input.</summary>
    /// <param name="inputName">Name of the media input</param>
    /// <param name="mediaCursor">New cursor position to set (&gt;= 0)</param>
    /// <remarks>
    /// This request does not perform bounds checking of the cursor position.
    /// </remarks>
    Task SetMediaInputCursor(string inputName, int mediaCursor);

    /// <summary>
    /// Offsets the current cursor position of a media input by the specified value.
    /// </summary>
    /// <param name="inputName">Name of the media input</param>
    /// <param name="mediaCursorOffset">Value to offset the current cursor position by</param>
    /// <remarks>
    /// This request does not perform bounds checking of the cursor position.
    /// </remarks>
    Task OffsetMediaInputCursor(string inputName, int mediaCursorOffset);

    /// <summary>Triggers an action on a media input.</summary>
    /// <param name="inputName">Name of the media input</param>
    /// <param name="mediaAction">Identifier of the ObsMediaInputAction enum</param>
    Task TriggerMediaInputAction(string inputName, ObsMediaInputAction mediaAction);

    /// <summary>Gets the status of the virtualcam output.</summary>
    /// <returns>Whether the output is active</returns>
    Task<bool> GetVirtualCamStatus();

    /// <summary>Toggles the state of the virtualcam output.</summary>
    /// <returns>Whether the output is active</returns>
    Task<bool> ToggleVirtualCam();

    /// <summary>Starts the virtualcam output.</summary>
    Task StartVirtualCam();

    /// <summary>Stops the virtualcam output.</summary>
    Task StopVirtualCam();

    /// <summary>Gets the status of the replay buffer output.</summary>
    /// <returns>Whether the output is active</returns>
    Task<bool> GetReplayBufferStatus();

    /// <summary>Toggles the state of the replay buffer output.</summary>
    /// <returns>Whether the output is active</returns>
    Task<bool> ToggleReplayBuffer();

    /// <summary>Starts the replay buffer output.</summary>
    Task StartReplayBuffer();

    /// <summary>Stops the replay buffer output.</summary>
    Task StopReplayBuffer();

    /// <summary>Saves the contents of the replay buffer output.</summary>
    Task SaveReplayBuffer();

    /// <summary>
    /// Gets the filename of the last replay buffer save file.
    /// </summary>
    /// <returns>File path</returns>
    Task<string> GetLastReplayBufferReplay();

    /// <summary>Gets the list of available outputs.</summary>
    /// <returns>Array of <see cref="T:OBSStudioClient.Classes.Output" /></returns>
    Task<Output[]> GetOutputList();

    /// <summary>Gets the status of an output.</summary>
    /// <param name="outputName">Output name</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.OutputStatusResponse" /></returns>
    Task<OutputStatusResponse> GetOutputStatus(string outputName);

    /// <summary>Toggles the status of an output.</summary>
    /// <param name="outputName">Output name</param>
    /// <returns>Whether the output is active</returns>
    Task<bool> ToggleOutput(string outputName);

    /// <summary>Starts an output.</summary>
    /// <param name="outputName">Output name</param>
    Task StartOutput(string outputName);

    /// <summary>Stops an output.</summary>
    /// <param name="outputName">Output name</param>
    Task StopOutput(string outputName);

    /// <summary>Gets the settings of an output.</summary>
    /// <param name="outputName">Output name</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.OutputSettingsResponse" /></returns>
    Task<Dictionary<string, object>> GetOutputSettings(string outputName);

    /// <summary>Sets the settings of an output.</summary>
    /// <param name="outputName">Output name</param>
    /// <param name="outputSettings">Output settings</param>
    Task SetOutputSettings(string outputName, Dictionary<string, object> outputSettings);

    /// <summary>Gets the status of the record output.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.RecordStatusResponse" /></returns>
    Task<RecordStatusResponse> GetRecordStatus();

    /// <summary>Toggles the status of the record output.</summary>
    Task<bool> ToggleRecord();

    /// <summary>Starts the record output.</summary>
    Task StartRecord();

    /// <summary>Stops the record output.</summary>
    /// <returns>File name for the saved recording</returns>
    Task<string> StopRecord();

    /// <summary>Toggles pause on the record output.</summary>
    Task<bool> ToggleRecordPause();

    /// <summary>Pauses the record output.</summary>
    Task PauseRecord();

    /// <summary>Resumes the record output.</summary>
    Task ResumeRecord();

    /// <summary>
    /// Sets the current directory that the record output writes files to.
    /// </summary>
    /// <param name="recordDirectory">The directory that the record output writes to.</param>
    Task SetRecordDirectory(string recordDirectory);

    /// <summary>Gets a list of all scene items in a scene.</summary>
    /// <param name="sceneName">Name of the scene to get the items of</param>
    /// <returns>Array of <see cref="T:OBSStudioClient.Classes.SceneItem" /> in the scene</returns>
    Task<SceneItem[]> GetSceneItemList(string sceneName);

    /// <summary>Basically GetSceneItemList, but for groups.</summary>
    /// <param name="sceneName">Name of the group to get the items of</param>
    /// <returns>Array of <see cref="T:OBSStudioClient.Classes.SceneItem" /> in the group</returns>
    /// <remarks>
    /// Using groups at all in OBS is discouraged, as they are very broken under the hood. Please use nested scenes instead.
    /// </remarks>
    Task<SceneItem[]> GetGroupSceneItemList(string sceneName);

    /// <summary>Searches a scene for a source, and returns its id.</summary>
    /// <param name="sceneName">Name of the scene or group to search in</param>
    /// <param name="sourceName">Name of the source to find</param>
    /// <param name="searchOffset">Number of matches to skip during search. &gt;= 0 means first forward. -1 means last (top) item (&gt;= -1)</param>
    /// <returns>Numeric ID of the scene item</returns>
    Task<int> GetSceneItemId(string sceneName, string sourceName, int searchOffset = 0);

    /// <summary>Creates a new scene item using a source.</summary>
    /// <param name="sceneName">Name of the scene to create the new item in</param>
    /// <param name="sourceName">Name of the source to add to the scene</param>
    /// <param name="sceneItemEnabled">Enable state to apply to the scene item on creation</param>
    /// <returns>Numeric ID of the scene item</returns>
    Task<int> CreateSceneItem(string sceneName, string sourceName, bool sceneItemEnabled = true);

    /// <summary>Removes a scene item from a scene.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    Task RemoveSceneItem(string sceneName, int sceneItemId);

    /// <summary>
    /// Duplicates a scene item, copying all transform and crop info.
    /// </summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <param name="destinationSceneName">Name of the scene to create the duplicated item in</param>
    /// <returns>Numeric ID of the duplicated scene item</returns>
    Task<int> DuplicateSceneItem(string sceneName, int sceneItemId, string? destinationSceneName = null);

    /// <summary>Gets the transform and crop info of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <returns>Object containing scene item transform info</returns>
    Task<SceneItemTransform> GetSceneItemTransform(string sceneName, int sceneItemId);

    /// <summary>Sets the transform and crop info of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <param name="sceneItemTransform">Object containing scene item transform info to update</param>
    Task SetSceneItemTransform(string sceneName, int sceneItemId, SceneItemTransform sceneItemTransform);

    /// <summary>Gets the enable state of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <returns>Whether the scene item is enabled. true for enabled, false for disabled</returns>
    Task<bool> GetSceneItemEnabled(string sceneName, int sceneItemId);

    /// <summary>Sets the enable state of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <param name="sceneItemEnabled">	New enable state of the scene item</param>
    Task SetSceneItemEnabled(string sceneName, int sceneItemId, bool sceneItemEnabled);

    /// <summary>Gets the lock state of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <returns>Whether the scene item is locked. true for locked, false for unlocked</returns>
    Task<bool> GetSceneItemLocked(string sceneName, int sceneItemId);

    /// <summary>Sets the lock state of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <param name="sceneItemLocked">New lock state of the scene item</param>
    Task SetSceneItemLocked(string sceneName, int sceneItemId, bool sceneItemLocked);

    /// <summary>Gets the index position of a scene item in a scene.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <returns></returns>
    /// <remarks>
    /// An index of 0 is at the bottom of the source list in the UI.
    /// Scenes and Groups
    /// </remarks>
    Task<int> GetSceneItemIndex(string sceneName, int sceneItemId);

    /// <summary>Sets the index position of a scene item in a scene.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <param name="sceneItemIndex">New index position of the scene item (&gt;= 0)</param>
    Task SetSceneItemIndex(string sceneName, int sceneItemId, int sceneItemIndex);

    /// <summary>Gets the blend mode of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <returns>Current blend mode</returns>
    Task<BlendMode> GetSceneItemBlendMode(string sceneName, int sceneItemId);

    /// <summary>Sets the blend mode of a scene item.</summary>
    /// <param name="sceneName">Name of the scene the item is in</param>
    /// <param name="sceneItemId">Numeric ID of the scene item (&gt;= 0)</param>
    /// <param name="sceneItemBlendMode">New blend mode</param>
    Task SetSceneItemBlendMode(string sceneName, int sceneItemId, BlendMode sceneItemBlendMode);

    /// <summary>Gets an array of all scenes in OBS.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SceneListResponse" /></returns>
    Task<SceneListResponse> GetSceneList();

    /// <summary>Gets an array of all groups in OBS.</summary>
    /// <returns>Array of group names</returns>
    /// <remarks>
    /// Groups in OBS are actually scenes, but renamed and modified. In obs-websocket, we treat them as scenes where we can.
    /// </remarks>
    Task<string[]> GetGroupList();

    /// <summary>Gets the current program scene.</summary>
    /// <returns>Current program scene</returns>
    Task<string> GetCurrentProgramScene();

    /// <summary>Sets the current program scene.</summary>
    /// <param name="sceneName">Scene to set as the current program scene</param>
    Task SetCurrentProgramScene(string sceneName);

    /// <summary>Gets the current preview scene.</summary>
    /// <returns>Current preview scene</returns>
    Task<string?> GetCurrentPreviewScene();

    /// <summary>Sets the current preview scene.</summary>
    /// <param name="sceneName">Scene to set as the current preview scene</param>
    Task SetCurrentPreviewScene(string sceneName);

    /// <summary>Creates a new scene in OBS.</summary>
    /// <param name="sceneName">Name for the new scene</param>
    Task CreateScene(string sceneName);

    /// <summary>Removes a scene from OBS.</summary>
    /// <param name="sceneName">Name of the scene to remove</param>
    Task RemoveScene(string sceneName);

    /// <summary>Sets the name of a scene (rename).</summary>
    /// <param name="sceneName">Name of the scene to be renamed</param>
    /// <param name="newSceneName">New name for the scene</param>
    Task SetSceneName(string sceneName, string newSceneName);

    /// <summary>Gets the scene transition overridden for a scene.</summary>
    /// <param name="sceneName">Name of the scene</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SceneTransitionResponse" /></returns>
    Task<SceneTransitionResponse> GetSceneSceneTransitionOverride(string sceneName);

    /// <summary>Gets the scene transition overridden for a scene.</summary>
    /// <param name="sceneName">Name of the scene</param>
    /// <param name="transitionName">Name of the scene transition to use as override. Specify null to remove.</param>
    /// <param name="transitionDuration">Duration to use for any overridden transition. Specify null to remove.</param>
    Task SetSceneSceneTransitionOverride(string sceneName, string? transitionName, int? transitionDuration);

    /// <summary>Gets the active and show state of a source.</summary>
    /// <param name="sourceName">Name of the source to get the active state of</param>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SourceActiveResponse" /></returns>
    /// <remarks>Compatible with inputs and scenes.</remarks>
    Task<SourceActiveResponse> GetSourceActive(string sourceName);

    /// <summary>Gets a Base64-encoded screenshot of a source.</summary>
    /// <param name="sourceName">Name of the source to take a screenshot of</param>
    /// <param name="imageFormat">Image compression format to use. Use GetVersion to get compatible image formats</param>
    /// <param name="imageWidth">Width to scale the screenshot to (between 8 and 4096)</param>
    /// <param name="imageHeight">Height to scale the screenshot to (between 8 and 4096)</param>
    /// <param name="imageCompressionQuality">Compression quality to use. 0 for high compression, 100 for uncompressed. -1 to use "default" (whatever that means, idk) (between -1 and 100)</param>
    /// <returns>Base64-encoded screenshot</returns>
    /// <remarks>
    /// The imageWidth and imageHeight parameters are treated as "scale to inner", meaning the smallest ratio will be used and the aspect ratio of the original resolution is kept. If imageWidth and imageHeight are not specified, the compressed image will use the full resolution of the source.
    /// Compatible with inputs and scenes.
    /// </remarks>
    Task<string> GetSourceScreenshot(string sourceName, string imageFormat, int? imageWidth = null, int? imageHeight = null, int? imageCompressionQuality = -1);

    /// <summary>Saves a screenshot of a source to the filesystem.</summary>
    /// <param name="sourceName">Name of the source to take a screenshot of</param>
    /// <param name="imageFormat">Image compression format to use. Use GetVersion to get compatible image formats</param>
    /// <param name="imageFilePath">Path to save the screenshot file to. Eg. C:\Users\user\Desktop\screenshot.png</param>
    /// <param name="imageWidth">Width to scale the screenshot to (between 8 and 4096)</param>
    /// <param name="imageHeight">Height to scale the screenshot to (between 8 and 4096)</param>
    /// <param name="imageCompressionQuality">Compression quality to use. 0 for high compression, 100 for uncompressed. -1 to use "default" (whatever that means, idk) (between -1 and 100)</param>
    /// <remarks>
    /// The imageWidth and imageHeight parameters are treated as "scale to inner", meaning the smallest ratio will be used and the aspect ratio of the original resolution is kept. If imageWidth and imageHeight are not specified, the compressed image will use the full resolution of the source.
    /// Compatible with inputs and scenes.
    /// </remarks>
    Task SaveSourceScreenshot(string sourceName, string imageFormat, string imageFilePath, int? imageWidth = null, int? imageHeight = null, int? imageCompressionQuality = -1);

    /// <summary>Gets the status of the stream output.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.OutputStatusResponse" /></returns>
    Task<OutputStatusResponse> GetStreamStatus();

    /// <summary>Toggles the status of the stream output.</summary>
    /// <returns>New state of the stream output</returns>
    Task<bool> ToggleStream();

    /// <summary>Starts the stream output.</summary>
    Task StartStream();

    /// <summary>Stops the stream output.</summary>
    Task StopStream();

    /// <summary>Sends CEA-608 caption text over the stream output.</summary>
    /// <param name="captionText">Caption text</param>
    Task SendStreamCaption(string captionText);

    /// <summary>Gets an array of all available transition kinds.</summary>
    /// <returns>Array of transition kinds</returns>
    Task<string[]> GetTransitionKindList();

    /// <summary>Gets an array of all scene transitions in OBS.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.SceneTransitionListResponse" /></returns>
    Task<SceneTransitionListResponse> GetSceneTransitionList();

    /// <summary>Gets information about the current scene transition.</summary>
    /// <returns>A <see cref="T:OBSStudioClient.Responses.TransitionResponse" /></returns>
    Task<TransitionResponse> GetCurrentSceneTransition();

    /// <summary>Sets the current scene transition.</summary>
    /// <param name="transitionName">Name of the transition to make active</param>
    /// <remarks>
    /// Small note: While the namespace of scene transitions is generally unique, that uniqueness is not a guarantee as it is with other resources like inputs.
    /// </remarks>
    Task SetCurrentSceneTransition(string transitionName);

    /// <summary>
    /// Sets the duration of the current scene transition, if it is not fixed.
    /// </summary>
    /// <param name="transitionDuration">Duration in milliseconds.</param>
    Task SetCurrentSceneTransitionDuration(float transitionDuration);

    /// <summary>Sets the settings of the current scene transition.</summary>
    /// <param name="transitionSettings">Settings object to apply to the transition. Can be {}</param>
    /// <param name="overlay">Whether to overlay over the current settings or replace them</param>
    Task SetCurrentSceneTransitionSettings(TransitionResponse? transitionSettings, bool overlay = true);

    /// <summary>
    /// Gets the cursor position of the current scene transition.
    /// </summary>
    /// <returns>Cursor position, between 0.0 and 1.0</returns>
    /// <remarks>
    /// Note: transitionCursor will return 1.0 when the transition is inactive.
    /// </remarks>
    Task<float> GetCurrentSceneTransitionCursor();

    /// <summary>
    /// Triggers the current scene transition. Same functionality as the Transition button in studio mode.
    /// </summary>
    Task TriggerStudioModeTransition();

    /// <summary>Sets the position of the TBar.</summary>
    /// <param name="position">New position.</param>
    /// <param name="release">Whether to release the TBar. Only set false if you know that you will be sending another position update</param>
    /// <remarks>
    /// Very important note: This will be deprecated and replaced in a future version of obs-websocket.
    /// </remarks>
    Task SetTBarPosition(float position, bool release = true);

    /// <summary>Gets whether studio is enabled.</summary>
    /// <returns>Whether studio mode is enabled</returns>
    Task<bool> GetStudioModeEnabled();

    /// <summary>Enables or disables studio mode.</summary>
    /// <param name="studioModeEnabled">True == Enabled, False == Disabled</param>
    Task SetStudioModeEnabled(bool studioModeEnabled);

    /// <summary>Opens the properties dialog of an input.</summary>
    /// <param name="inputName">Name of the input to open the dialog of</param>
    Task OpenInputPropertiesDialog(string inputName);

    /// <summary>Opens the filters dialog of an input.</summary>
    /// <param name="inputName">Name of the input to open the dialog of</param>
    Task OpenInputFiltersDialog(string inputName);

    /// <summary>Opens the interact dialog of an input.</summary>
    /// <param name="inputName">Name of the input to open the dialog of</param>
    Task OpenInputInteractDialog(string inputName);

    /// <summary>
    /// Gets a list of connected monitors and information about them.
    /// </summary>
    /// <returns>a list of detected monitors with some information</returns>
    Task<Monitor[]> GetMonitorList();

    /// <summary>Opens a projector for a specific output video mix.</summary>
    /// <param name="videoMixType">Type of mix to open</param>
    /// <param name="monitorIndex">Monitor index, use GetMonitorList to obtain index. Use -1 for windowed mode</param>
    /// <remarks>Note: This request serves to provide feature parity with 4.x. It is very likely to be changed/deprecated in a future release.</remarks>
    Task OpenVideoMixProjectorOnMonitor(MixType videoMixType, int monitorIndex);

    /// <summary>Opens a projector for a specific output video mix.</summary>
    /// <param name="videoMixType">Type of mix to open</param>
    /// <param name="projectorGeometry">Size/Position data for a windowed projector, in Qt Base64 encoded format.  See <see cref="M:OBSStudioClient.ObsClient.GetGeometry(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Boolean,System.Boolean,System.Int32)" />.</param>
    /// <remarks>Note: This request serves to provide feature parity with 4.x. It is very likely to be changed/deprecated in a future release.</remarks>
    Task OpenVideoMixProjectorWindow(MixType videoMixType, string projectorGeometry);

    /// <summary>Opens a projector for a source.</summary>
    /// <param name="sourceName">Name of the source to open a projector for</param>
    /// <param name="monitorIndex">Monitor index, use GetMonitorList to obtain index. Use -1 for windowed mode.</param>
    /// <remarks>Note: This request serves to provide feature parity with 4.x. It is very likely to be changed/deprecated in a future release.</remarks>
    Task OpenSourceProjectorOnMonitor(string sourceName, int monitorIndex);

    /// <summary>Opens a projector for a source.</summary>
    /// <param name="sourceName">Name of the source to open a projector for</param>
    /// <param name="projectorGeometry">Size/Position data for a windowed projector, in Qt Base64 encoded format. See <see cref="M:OBSStudioClient.ObsClient.GetGeometry(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Boolean,System.Boolean,System.Int32)" />.</param>
    /// <remarks>Note: This request serves to provide feature parity with 4.x. It is very likely to be changed/deprecated in a future release.</remarks>
    Task OpenSourceProjectorWindow(string sourceName, string projectorGeometry);

}