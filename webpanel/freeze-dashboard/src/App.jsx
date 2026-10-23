import { API_BASE_URL } from './config';
import { useState, useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Power,
  RotateCcw,
  Trash2,
  Wifi,
  Terminal,
  Server,
  AlertTriangle,
  Loader2,
  Zap,
  Activity,
  SquarePen,
  FolderGit2
} from 'lucide-react';

function StatusBadge({ connected, username = "" }) {
  return (
    <motion.div
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex items-center gap-2 px-4 py-2 rounded-lg border bg-[#0c0c0e]/80 backdrop-blur-sm"
      style={{ borderColor: '#27272a' }}
    >
      <motion.div
        className="relative flex h-3 w-3"
        animate={connected ? { scale: [1, 1.2, 1] } : {}}
        transition={connected ? { duration: 1.5, repeat: Infinity, ease: 'easeInOut' } : {}}
      >
        <div
          className="absolute h-3 w-3 rounded-full"
          style={{
            backgroundColor: connected ? '#22c55e' : '#ef4444',
            boxShadow: connected ? '0 0 12px #22c55e' : '0 0 12px #ef4444',
          }}
        />
        {connected && (
          <motion.div
            className="absolute h-3 w-3 rounded-full"
            style={{ backgroundColor: '#22c55e', opacity: 0.4 }}
            animate={{ scale: [1, 2.5], opacity: [0.4, 0] }}
            transition={{ duration: 1.5, repeat: Infinity, ease: 'easeOut' }}
          />
        )}
      </motion.div>
      <span className="font-mono text-sm font-medium text-neutral-100">
        {connected ? 'Connected' : 'Disconnected'}
      </span>

      {username && (
      <span className="font-mono text-xs px-2 py-0.5 rounded bg-[#1f1f22] border" style={{ color: '#a1a1aa', borderColor: '#27272a' }}>
        {username}
      </span>
      )}
    </motion.div>
  );
}

function NetworkToggle({ network, isActive, onToggle }) {
  return (
    <button
      onClick={() => onToggle(network.id)}
      className="relative flex items-center justify-between w-full pl-4 pr-3.5 py-2.5 rounded-lg bg-neutral-900/60 border border-neutral-800/80 hover:border-neutral-700 transition-all duration-200 active:scale-95"
    >
      <div className="flex items-center gap-3">
        <span className={`w-2.5 h-2.5 rounded-full flex-shrink-0 ${isActive ? 'bg-emerald-400 shadow-[0_0_8px_rgba(52,211,153,0.8)]' : 'bg-neutral-700'}`} />
        <span className="font-mono text-xs font-semibold text-neutral-200 tracking-wider">
          {network.name}
        </span>
      </div>
      <div className={`w-7 h-4 rounded-full border relative transition-all duration-200 ${isActive ? 'bg-emerald-500/20 border-emerald-500/40' : 'bg-neutral-800 border-neutral-700'}`}>
        <div className={`w-3 h-3 rounded-full transition-all duration-200 absolute top-0.5 ${isActive ? 'right-0.5 bg-emerald-400' : 'left-0.5 bg-neutral-600'}`} />
      </div>
    </button>
  );
}

function InputField({ label, value, onChange, type = 'text', placeholder, icon: Icon }) {
  return (
    <div className="w-full">
      <label className="block font-mono text-xs mb-2 text-neutral-500">
        {label}
      </label>
      <div className="relative">
        {Icon && (
          <div className="absolute left-3 top-1/2 -translate-y-1/2" style={{ color: '#71717a' }}>
            <Icon size={16} />
          </div>
        )}
        <input
          type={type}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          className="w-full px-4 py-3 rounded-lg border font-mono text-sm transition-all duration-200 outline-none bg-[#0a0a0a] border-neutral-800 text-neutral-100 placeholder-neutral-600"
          style={{
            paddingLeft: Icon ? '40px' : '16px',
          }}
          onFocus={(e) => {
            e.target.style.borderColor = '#3f3f46';
            e.target.style.boxShadow = '0 0 0 2px rgba(63, 63, 70, 0.3)';
          }}
          onBlur={(e) => {
            e.target.style.borderColor = '#27272a';
            e.target.style.boxShadow = 'none';
          }}
        />
      </div>
    </div>
  );
}

function ActionButton({ children, onClick, variant = 'primary', disabled, loading, icon: Icon, className = '' }) {
  const baseStyles = 'relative px-4 py-2 mx-1 rounded-lg border border-neutral-800 text-xs font-semibold flex items-center justify-center gap-2.5 transition-all duration-200';
  
  const variants = {
    primary: 'bg-neutral-900 text-neutral-100 hover:bg-neutral-800 active:scale-95',
    secondary: 'bg-[#0c0c0e]/80 backdrop-blur-sm text-neutral-100 hover:bg-neutral-900/80 active:scale-95',
    danger: 'bg-[#0c0c0e]/80 backdrop-blur-sm text-red-400 border-red-500/40 hover:bg-red-500/10 active:scale-95',
  };

  return (
    <motion.button
      onClick={onClick}
      disabled={disabled || loading}
      whileHover={!disabled && !loading ? { scale: 1.01 } : {}}
      whileTap={!disabled && !loading ? { scale: 0.98 } : {}}
      className={`${baseStyles} ${variants[variant]} ${className}`}
      style={{ opacity: disabled ? 0.5 : 1, cursor: disabled ? 'not-allowed' : 'pointer' }}
    >
      {loading && (
        <motion.div
          className="h-4 w-4"
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
        >
          <Loader2 size={12} className="text-current" />
        </motion.div>
      )}
      {!loading && Icon && <Icon size={12} />}
      <span>{children}</span>
    </motion.button>
  );
}

function LogEntry({ log }) {
  const levelColors = {
    INFO: '#22c55e',
    DEBUG: '#3b82f6',
    WARN: '#f59e0b',
    ERROR: '#ef4444',
  };

  const levelBg = {
    INFO: 'bg-emerald-500/10',
    DEBUG: 'bg-blue-500/10',
    WARN: 'bg-amber-500/10',
    ERROR: 'bg-red-500/10',
  };

  return (
    <motion.div
      key={log.id}
      initial={{ opacity: 0, x: -20 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: 20 }}
      transition={{ duration: 0.2 }}
      className="flex items-start gap-3 px-4 py-1 hover:bg-neutral-900/50 transition-colors border-l-2"
      style={{ borderLeftColor: levelColors[log.level] || '#27272a' }}
    >
      <span className="font-mono text-xs shrink-0 py-0.5 text-neutral-500 min-w-[70px]">
        [{log.timestamp}]
      </span>
      <span
        className={`font-mono text-xs shrink-0 px-2 py-0.5 rounded ${levelBg[log.level] || 'bg-neutral-800'}`}
        style={{ color: levelColors[log.level] || '#fafafa' }}
      >
        {log.level}
      </span>
      <span className="font-mono text-xs break-all py-0.5 text-neutral-300 flex-1">
        {log.message}
      </span>
    </motion.div>
  );
}

function ConsoleLog({ logs, isRunning, onClear }) {
  const logContainerRef = useRef(null);
  const [autoScroll, setAutoScroll] = useState(true);

  useEffect(() => {
    if (autoScroll && logContainerRef.current) {
      logContainerRef.current.scrollTop = logContainerRef.current.scrollHeight;
    }
  }, [logs, autoScroll]);

  const handleScroll = useCallback((e) => {
    const { scrollTop, scrollHeight, clientHeight } = e.target;
    setAutoScroll(scrollTop + clientHeight >= scrollHeight - 50);
  }, []);

  return (
    <div className="flex flex-col h-full bg-[#050507] border border-neutral-800/80 rounded-xl overflow-hidden flex flex-col shadow-2xl">
      <div className="flex items-center justify-between px-4 py-3 border-b" style={{ borderColor: '#27272a' }}>
        <div className="flex items-center gap-2">
          <Terminal size={14} className="text-neutral-500" />
          <span className="font-mono text-sm text-neutral-100">
            Worker Console
          </span>
          {isRunning && (
            <motion.div
              className="flex items-center gap-1"
              animate={{ opacity: [1, 0.5, 1] }}
              transition={{ duration: 1, repeat: Infinity }}
            >
              <Activity size={10} className="text-emerald-400" />
              <span className="font-mono text-xs text-emerald-400">LIVE</span>
            </motion.div>
          )}
        </div>
        <motion.button
          onClick={onClear}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-mono transition-colors bg-neutral-900/60 border border-neutral-800 text-neutral-500 hover:text-neutral-100 hover:border-neutral-700"
        >
          <Trash2 size={12} />
          Clear
        </motion.button>
      </div>
      <div
        ref={logContainerRef}
        onScroll={handleScroll}
        className="flex-1 overflow-y-auto scrollbar-thin p-4 space-y-1"
        style={{ minHeight: 0 }}
      >
        <AnimatePresence mode="popLayout">
          {logs.map((log) => (
            <LogEntry key={log.id} log={log} />
          ))}
        </AnimatePresence>
        {!autoScroll && (
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="absolute bottom-4 right-4 z-10"
            onClick={() => setAutoScroll(true)}
          >
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-mono shadow-lg bg-neutral-900/80 border border-emerald-500 text-emerald-400"
            >
              <RotateCcw size={12} />
              Scroll to bottom
            </motion.button>
          </motion.div>
        )}
      </div>
    </div>
  );
}

export default function Dashboard() {
  const [connected, setConnected] = useState(false);
  const [activeNetworks, setActiveNetworks] = useState(new Set());
  const [sourceChannel, setSourceChannel] = useState('');
  const [messageDelay, setMessageDelay] = useState(400);
  const [isRunning, setIsRunning] = useState(false);
  const [username, setUsername] = useState(""); 
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isAuthOpen, setIsAuthOpen] = useState(false);
  const [authCode, setAuthCode] = useState("");
  const [networks, setNetworks] = useState([]);
  const [apiHashModal, setApiHashModal] = useState("");
  const [sourceChannelModal, setSourceChannelModal] = useState('');
  const [messageDelayModal, setMessageDelayModal] = useState(400);
  const [apiIdModal, setApiIdModal] = useState("");
  const [phoneNumberModal, setPhoneNumberModal] = useState("");
  const [passwordModal, setPasswordModal] = useState("");
  const [logs, setLogs] = useState([
    { id: 1, timestamp: new Date().toTimeString().slice(0, 8), level: 'INFO', message: 'Dashboard UI initialized. Waiting for connection...' }
  ]);
  const [starting, setStarting] = useState(false);
  
  const logIdRef = useRef(2);
  const connectionRef = useRef(null);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/logs`)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.on("ReceiveLog", (message) => {
      const now = new Date().toTimeString().slice(0, 8);
      
      let level = 'INFO';
      if (message.includes('ERROR')) level = 'ERROR';
      else if (message.includes('WARN')) level = 'WARN';
      else if (message.includes('DEBUG')) level = 'DEBUG';

      const newLog = {
        id: logIdRef.current++,
        timestamp: now,
        level: level,
        message: message
      };

      setLogs(prev => [...prev.slice(-199), newLog]);
    });

    
    connection.on("AuthCode", (message) => {
      setIsAuthOpen(true);
    });

    connection.on("SetUsername", (message) => {
      setUsername(message);
    });

    connection.start()
      .then(() => {
        setConnected(true);

        const newLog = {
          id: logIdRef.current++,
          timestamp: new Date().toTimeString().slice(0, 8),
          level: 'INFO',
          message: "Server Connected!"
        }

        setLogs(prev => [...prev.slice(-199), newLog]);
        console.log("SignalR Connected!");
      })
      .catch(err => {
        setConnected(false);
        console.error("SignalR Connection Error: ", err);
      });

    return () => {
      connection.stop();
    };
  }, []);

  useEffect(() => {
    fetchConfig();
    fetchNetworks();
  }, []);

  const fetchNetworks = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/worker/getnet`);
      const data = await response.json();
      
      if (data.message == "No networks found") {
        throw new Error("No networks found");
      }
      else {
        const formatted = data.message.map(item => ({ id: item, name: item }));
        setNetworks(formatted);
      }
    } catch (error) {
      console.error('Error with update networks list:', error);
    }
  };
  
  const fetchConfig = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/worker/getcfg`);
      const data = await response.json();
      
      if (data.message == "Can't read config.yml") {
        throw new Error("Can't read config.yml");
      }
      else {
        const [apiHash, sourceId, msgDelay, apiId, phoneNumber, password] = data.message;
        setApiHashModal(apiHash);
        setSourceChannel(sourceId);
        setSourceChannelModal(sourceId);
        setMessageDelay(msgDelay);
        setMessageDelayModal(msgDelay);
        setApiIdModal(apiId);
        setPhoneNumberModal(phoneNumber);
        setPasswordModal(password);
      }
    } catch (error) {
      console.error('Error with update networks list:', error);
    }
  };

  const writeConfig = useCallback(async () => {
    if (isRunning) return;

    setSourceChannel(sourceChannelModal);
    setMessageDelay(messageDelayModal);

    try {
    const res = await fetch(`${API_BASE_URL}/api/worker/writecfg`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        apiHash: apiHashModal,
        sourceId: sourceChannelModal,
        messageDelay: messageDelayModal,
        apiId: apiIdModal,
        phoneNumber: phoneNumberModal,
        password: passwordModal,
      })
    });

    const data = await res.json();

    setLogs(prev => [
      ...prev,
      { 
        id: logIdRef.current++, 
        timestamp: new Date().toTimeString().slice(0, 8), 
        level: res.ok ? 'INFO' : 'ERROR', 
        message: data.message 
      }
    ]);
  } catch (error) {
    console.error('API Error:', error);
    setLogs(prev => [
      ...prev,
      { 
        id: logIdRef.current++, 
        timestamp: new Date().toTimeString().slice(0, 8), 
        level: 'ERROR', 
        message: `Network Error: ${error.message}` 
      }
    ]);
  }
  }, [isRunning, apiHashModal, sourceChannelModal, messageDelayModal, apiIdModal, phoneNumberModal, passwordModal]);
  
  const handleChannelChangeModal = useCallback((e) => {
    let value = e.target.value;
    
    value = value.replace(/[^\d-]/g, '');
    
    if (value.indexOf('-') > 0) {
      value = value.replace(/-/g, '');
    }

    setSourceChannelModal(value);
  }, []);

  
  const handleApiIdChangeModal = useCallback((e) => {
    let value = e.target.value;
    
    value = value.replace(/[^\d-]/g, '');
    
    if (value.indexOf('-') > 0) {
      value = value.replace(/-/g, '');
    }

    setApiIdModal(value);
  }, []);

  
  const handleDelayChangeModal = useCallback((e) => {
    let value = e.target.value;
    
    value = value.replace(/[^\d-]/g, '');
    
    if (value.indexOf('-') > 0) {
      value = value.replace(/-/g, '');
    }

    setMessageDelayModal(value);
  }, []);

  const toggleNetwork = useCallback((networkId) => {
    setActiveNetworks(prev => {
      const next = new Set(prev);
      if (next.has(networkId)) next.delete(networkId);
      else next.add(networkId);
      return next;
    });
  }, []);

  const startWorker = useCallback(async () => {
    if (isRunning || starting) return;
    if (sourceChannel.trim() == "") {
      setLogs(prev => [
          ...prev,
          { id: logIdRef.current++, timestamp: new Date().toTimeString().slice(0, 8), level: 'ERROR', message: "Source Channel can't be empty" }
        ]);
      return;
    }
    setStarting(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/worker/start`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          sourceChannel: sourceChannel,
          delay: messageDelay,
          networks: Array.from(activeNetworks)
        })
      });

      if (response.ok) {
        setIsRunning(true);
        setLogs(prev => [
          ...prev,
          { id: logIdRef.current++, timestamp: new Date().toTimeString().slice(0, 8), level: 'INFO', message: 'Worker started successfully via API' }
        ]);
      } else {
        alert('Ошибка запуска воркер-процесса на сервере');
      }
    } catch (error) {
      console.error('API Error:', error);
    } finally {
      setStarting(false);
    }
  }, [isRunning, starting, sourceChannel, messageDelay, activeNetworks]);

  const stopWorker = useCallback(async () => {
    if (!isRunning) return;

    try {
      await fetch(`${API_BASE_URL}/api/worker/stop`, { method: 'POST' });
      setIsRunning(false);
      setLogs(prev => [
        ...prev,
        { id: logIdRef.current++, timestamp: new Date().toTimeString().slice(0, 8), level: 'WARN', message: 'Worker stopped by user' }
      ]);
    } catch (error) {
      console.error('API Error:', error);
    }
  }, [isRunning]);

  const clearLogs = useCallback(() => {
    setLogs([]);
    logIdRef.current = 1;
  }, []);

  const resetConfig = useCallback(async () => {
  try {
    const res = await fetch(`${API_BASE_URL}/api/worker/resetcfg`, { 
      method: 'POST' 
    });
    
    const data = await res.json();

    setLogs(prev => [
      ...prev,
      { 
        id: logIdRef.current++, 
        timestamp: new Date().toTimeString().slice(0, 8), 
        level: res.ok ? 'INFO' : 'ERROR', 
        message: data.message || 'Configuration reset' 
      }
    ]);
  } catch (error) {
    console.error('API Error:', error);
    setLogs(prev => [
      ...prev,
      { 
        id: logIdRef.current++, 
        timestamp: new Date().toTimeString().slice(0, 8), 
        level: 'ERROR', 
        message: `Network error: ${error.message}` 
      }
    ]);
  }
}, []);

const handleSendCode = async () => {
    try {
      await fetch(`${API_BASE_URL}/api/worker/submit-code`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ authCode })
      });
      setAuthCode("");
    } catch (err) {
      console.error("Ошибка отправки кода:", err);
    }
  };

  const emergencyStop = useCallback(async () => {
    await stopWorker();
    setActiveNetworks(new Set());
    setLogs(prev => [
      ...prev,
      { id: logIdRef.current++, timestamp: new Date().toTimeString().slice(0, 8), level: 'ERROR', message: 'EMERGENCY STOP TRIGGERED' }
    ]);
  }, [stopWorker]);
  
  const activeCount = activeNetworks.size;

  return (
    <div className="w-full min-h-screen bg-[#080809] text-neutral-100 p-8 flex flex-col font-sans">
      <header className="sticky top-0 z-50 bg-[#0c0c0e]/80 backdrop-blur-md border-b border-neutral-800/80 px-8 py-4 flex items-center justify-between mb-8 rounded-xl">
        <div className="flex items-center gap-3 flex-shrink-0">
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            className="flex items-center justify-center h-9 w-9 rounded-lg"
            style={{ backgroundColor: 'rgba(34, 197, 94, 0.15)' }}
          >
            <Zap size={18} className="text-emerald-400" />
          </motion.div>
          <div>
            <h1 className="font-semibold text-sm tracking-tight text-neutral-100">
              TelegramHubRouter
            </h1>
          </div>
        </div>

        <div className="flex-1 flex justify-center absolute left-1/2 -translate-x-1/2">
          <StatusBadge connected={connected} username={username} />
        </div>

        <div className="flex items-center gap-4 flex-shrink-0">
          <a href='https://github.com/ciperv/TelegramHubRouter' target="_blank">
            <ActionButton
              icon={FolderGit2}
            >
              GitHub
            </ActionButton>
          </a>
        </div>
      </header>

      <main className="flex-1 w-full">
        <div className="grid grid-cols-12 gap-8 w-full max-w-[1920px] mx-auto">
          {/* Left Panel: Network Selector & Config */}
          <div className="col-span-12 xl:col-span-4 flex flex-col gap-6">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="bg-neutral-950/60 backdrop-blur-sm rounded-xl border border-neutral-800/80 flex flex-col"
            >
              <div className="flex items-center gap-2.5 px-6 pt-6 pb-2 text-xs font-semibold tracking-wider text-neutral-400 uppercase">
                <Wifi className="w-4 h-4 text-emerald-400" />
                <span>NETWORK SELECTOR</span>
              </div>
              <div className="flex items-center justify-end px-6 pb-2 text-xs font-mono text-neutral-500">
                <span className="px-2 py-0.5 rounded bg-emerald-500/10 border border-emerald-500/30 text-emerald-400">
                  {activeCount}/{networks.length} Active
                </span>
              </div>
              <div className="grid grid-cols-3 gap-3 p-6 pt-3">
                {networks.map((network) => (
                  <NetworkToggle
                    key={network.id}
                    network={network}
                    isActive={activeNetworks.has(network.id)}
                    onToggle={toggleNetwork}
                  />
                ))}
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.15 }}
              className="bg-neutral-950/60 backdrop-blur-sm rounded-xl border border-neutral-800/80 flex flex-col"
            >
              <div className="flex items-center gap-2.5 px-6 pt-6 pb-2 text-xs font-semibold tracking-wider text-neutral-400 uppercase">
                <Server className="w-4 h-4 text-emerald-400" />
                <span>WORKER CONFIGURATION</span>
              </div>
              <div className="px-6 pb-6 space-y-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                  <ActionButton
                    onClick={() => {
                      setIsModalOpen(true); 
                      fetchConfig();}}
                    disabled={isRunning}
                    loading={starting}
                    variant="primary"
                    icon={SquarePen}
                    className="flex-1"
                  >
                    Edit
                  </ActionButton>
                  <ActionButton
                    onClick={resetConfig}
                    disabled={isRunning}
                    variant="secondary"
                    icon={RotateCcw}
                    className="flex-1"
                  >
                    Reset
                  </ActionButton>
                </div>
              </div>
            </motion.div>

            {/* Worker Controls Card */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
              className="bg-neutral-950/60 backdrop-blur-sm rounded-xl border border-neutral-800/80 flex flex-col"
            >
              <div className="flex items-center gap-2.5 px-6 pt-6 pb-2 text-xs font-semibold tracking-wider text-neutral-400 uppercase">
                <Terminal className="w-4 h-4 text-emerald-400" />
                <span>WORKER CONTROLS</span>
              </div>
              <div className="px-6 pb-6">
                <div className="flex flex-col sm:flex-row gap-3">
                  <ActionButton
                    onClick={startWorker}
                    disabled={isRunning || starting || activeCount === 0}
                    loading={starting}
                    variant="primary"
                    icon={Power}
                    className="flex-1"
                  >
                    {isRunning ? 'Worker Running' : starting ? 'Starting...' : 'Start Worker'}
                  </ActionButton>
                  <ActionButton
                    onClick={stopWorker}
                    disabled={!isRunning}
                    variant="secondary"
                    icon={RotateCcw}
                    className="flex-1"
                  >
                    Stop Worker
                  </ActionButton>
                </div>
                {(isRunning || starting) && (
                  <motion.div
                    initial={{ opacity: 0, height: 0 }}
                    animate={{ opacity: 1, height: 'auto' }}
                    className="mt-6 p-4 rounded-lg flex items-center gap-2 text-xs font-mono border border-emerald-500/30 bg-emerald-500/10"
                  >
                    <motion.div
                      animate={{ scale: [1, 1.2, 1] }}
                      transition={{ duration: 1, repeat: Infinity }}
                      className="h-2 w-2 rounded-full bg-emerald-400"
                    />
                    <span className="text-emerald-400">Worker active — streaming logs to console</span>
                  </motion.div>
                )}
              </div>
            </motion.div>
          </div>

          {/* Right Panel: Console Log */}
          <div className="col-span-12 xl:col-span-8 flex flex-col h-[calc(100vh-175px)] min-h-[500px]">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.25 }}
              className="flex flex-col h-full"
            >
              <ConsoleLog logs={logs} isRunning={isRunning} onClear={clearLogs} />
            </motion.div>
          </div>
        </div>
      </main>

      {/* Mobile Action Bar */}
      <div className="lg:hidden fixed bottom-0 left-0 right-0 border-t backdrop-blur-md p-4 mt-auto bg-[#0c0c0e]/95 border-neutral-800/80">
        <div className="w-full max-w-[1920px] mx-auto flex items-center gap-3 px-8">
          <ActionButton
            onClick={emergencyStop}
            variant="danger"
            icon={AlertTriangle}
            className="flex-1"
          >
            Emergency Stop
          </ActionButton>
        </div>
      </div>

      {/* МОДАЛЬНОЕ ОКНО ТЕПЕРЬ ТУТ (на корневом уровне) */}
      <AnimatePresence>
        {isModalOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setIsModalOpen(false)}
              className="absolute inset-0 bg-black/70 backdrop-blur-sm"
            />
            <motion.div 
              initial={{ opacity: 0, scale: 0.95, y: 10 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 10 }}
              className="relative w-full max-w-md bg-[#121215] border border-[#27272a] rounded-2xl p-6 shadow-2xl z-10"
            >
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold text-neutral-100">Worker Config ("config.yml")</h3>
                <button 
                  onClick={() => setIsModalOpen(false)}
                  className="text-neutral-400 hover:text-white transition-colors"
                >
                  ✕
                </button>
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block text-xs text-neutral-400 mb-1">API_HASH <a href='https://my.telegram.org' target='_blank' className='hover:text-emerald-300'>(my.telegram.org)</a></label>
                  <input 
                    type="text" 
                    onChange={(e) => setApiHashModal(e.target.value)}
                    value={apiHashModal}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-xs text-neutral-400 mb-1">SOURCE_ID</label>
                  <input 
                    type="text" 
                    onChange={handleChannelChangeModal}
                    value={sourceChannelModal}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-xs text-neutral-400 mb-1">MSG_DELAY</label>
                  <input 
                    type="text" 
                    onChange={handleDelayChangeModal}
                    value={messageDelayModal}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-xs text-neutral-400 mb-1">API_ID <a href='https://my.telegram.org' target='_blank' className='hover:text-emerald-300'>(my.telegram.org)</a></label>
                  <input 
                    type="text" 
                    onChange={handleApiIdChangeModal}
                    value={apiIdModal}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-xs text-neutral-400 mb-1">PHONE_NUMBER</label>
                  <input 
                    type="text" 
                    onChange={(e) => setPhoneNumberModal(e.target.value)}
                    value={phoneNumberModal}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-xs text-neutral-400 mb-1">PASSWORD</label>
                  <input 
                    type="text" 
                    onChange={(e) => setPasswordModal(e.target.value)}
                    value={passwordModal}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div className="flex justify-end space-x-2 pt-2">
                  <button 
                    onClick={() => { 
                      setIsModalOpen(false);
                      fetchConfig();
                    }}
                    className="px-4 py-2 text-sm bg-neutral-800 hover:bg-neutral-700 text-neutral-200 rounded-lg transition-colors"
                  >
                    Cancel
                  </button>
                  <button 
                    onClick={() => { 
                      setIsModalOpen(false);
                      writeConfig();
                    }}
                    className="px-4 py-2 text-sm bg-emerald-600 hover:bg-emerald-500 text-white rounded-lg transition-colors"
                  >
                    Save
                  </button>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
      {/* Modal Auth Code */}
      <AnimatePresence>
        {isAuthOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="absolute inset-0 bg-black/70 backdrop-blur-sm"
            />
            <motion.div 
              initial={{ opacity: 0, scale: 0.95, y: 10 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 10 }}
              className="relative w-full max-w-md bg-[#121215] border border-[#27272a] rounded-2xl p-6 shadow-2xl z-10"
            >
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-semibold text-neutral-100">Enter code from Telegram:</h3>
                <button 
                  onClick={() => setIsAuthOpen(false)}
                  className="text-neutral-400 hover:text-white transition-colors"
                >
                  ✕
                </button>
              </div>
              <div className="space-y-4">
                <div>
                  <input 
                    type="text" 
                    onChange={(e) => setAuthCode(e.target.value)}
                    value={authCode}
                    placeholder="Enter a value." 
                    className="w-full px-3 py-2 bg-[#1f1f22] border border-[#27272a] rounded-lg text-neutral-100 text-sm focus:outline-none focus:border-emerald-500 transition-colors"
                  />
                </div>
                <div className="flex justify-end space-x-2 pt-2">
                  <button 
                    onClick={() => {
                      stopWorker();
                      setIsAuthOpen(false);
                    }}
                    className="px-4 py-2 text-sm bg-neutral-800 hover:bg-neutral-700 text-neutral-200 rounded-lg transition-colors"
                  >
                    Cancel
                  </button>
                  <button 
                    onClick={() => { 
                      handleSendCode();
                      setIsAuthOpen(false);
                    }}
                    className="px-4 py-2 text-sm bg-emerald-600 hover:bg-emerald-500 text-white rounded-lg transition-colors"
                  >
                    Send
                  </button>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
}