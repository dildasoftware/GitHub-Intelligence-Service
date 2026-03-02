
import { useState } from 'react';
import './App.css';
import { DeveloperService } from './services/api';
import type { DeveloperDto, AnalysisHistoryDto, DeveloperComparison } from './models/Developer';
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer, LineChart, Line, XAxis, YAxis, CartesianGrid } from 'recharts';
import { FaGithub, FaSearch, FaTrophy, FaMedal, FaBug, FaCode, FaDownload, FaChartLine, FaRocket, FaUserFriends, FaBalanceScale } from 'react-icons/fa';

function App() {
  const [username, setUsername] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // VERSUS MODE STATE
  const [isVersus, setIsVersus] = useState(false);
  const [username2, setUsername2] = useState('');
  const [comparisonData, setComparisonData] = useState<DeveloperComparison | null>(null);

  const [data, setData] = useState<DeveloperDto | null>(null);
  const [history, setHistory] = useState<AnalysisHistoryDto[]>([]);

  const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

  const handleAnalyze = async () => {
    if (!username.trim()) return;
    if (isVersus && !username2.trim()) return;

    setLoading(true);
    setError(null);
    setData(null);
    setComparisonData(null);
    setHistory([]);

    try {
      if (isVersus) {
        // --- VERSUS MODE ---
        const result = await DeveloperService.compareDevelopers(username, username2);
        setComparisonData(result);
      } else {
        // --- SINGLE MODE ---
        const result = await DeveloperService.analyzeDeveloper(username);
        setData(result);

        // Tarihçe sadece tekli modda anlamlı
        try {
          const historyData = await DeveloperService.getHistory(username);
          const formattedHistory = historyData.map(h => ({
            ...h,
            date: new Date(h.date).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })
          }));
          setHistory(formattedHistory);
        } catch (historyErr) {
          console.warn("Geçmiş verisi çekilemedi:", historyErr);
        }
      }

    } catch (err: any) {
      setError(err.response?.data?.message || 'Bir hata oluştu. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadReport = () => {
    if (data) {
      window.open(DeveloperService.getExportUrl(data.username), '_blank');
    }
  };

  // --- RENDER HELPERS ---

  const renderSingleAnalysis = () => {
    if (!data) return null;

    const chartData = data?.languageDistribution
      ? Object.entries(data.languageDistribution).map(([name, value]) => ({ name, value }))
      : [];

    return (
      <div className="dashboard-grid fade-in">
        {/* KART 1: TOTAL SCORE */}
        <div className="card score-card">
          <div className="card-header">
            <div className="icon-box bg-blue-light text-blue"><FaTrophy /></div>
            <span className="card-title">Total Score</span>
          </div>
          <div className="card-value">{data.totalScore.toFixed(1)}</div>
          <div className="progress-bar-bg">
            <div className="progress-bar-fill bg-blue" style={{ width: `${Math.min(data.totalScore, 100)}%` }}></div>
          </div>
        </div>

        {/* KART 2: SENIORITY */}
        <div className="card score-card">
          <div className="card-header">
            <div className="icon-box bg-purple-light text-purple"><FaMedal /></div>
            <span className="card-title">Seniority Level</span>
          </div>
          <div className="card-value text-gradient">{data.seniorityLevel}</div>
          <div className="chip">Professional</div>
        </div>

        {/* KART 3: QUALITY */}
        <div className="card score-card">
          <div className="card-header">
            <div className="icon-box bg-green-light text-green"><FaCode /></div>
            <span className="card-title">Code Quality</span>
          </div>
          <div className="card-value text-green">{data.qualityScore.toFixed(0)}</div>
          <div className="progress-bar-bg">
            <div className="progress-bar-fill bg-green" style={{ width: `${data.qualityScore}%` }}></div>
          </div>
        </div>

        {/* YENİ: KARİYER KARTI (AI GENERATED) */}
        {data.careerProfile && (
          <div className="card career-card" style={{ gridColumn: 'span 3' }}>
            <div className="career-header">
              <div className="icon-box bg-purple-light text-purple"><FaRocket /></div>
              <div>
                <span className="card-title">AI Career Recommendation</span>
                <h2 className="career-title">{data.careerProfile.title}</h2>
              </div>
            </div>
            <p className="career-desc">{data.careerProfile.description}</p>
            <div className="role-tags">
              {data.careerProfile.suitableRoles.map((role, i) => (
                <span key={i} className="role-chip">{role}</span>
              ))}
            </div>

            {/* TAVSİYELER (SMART TIPS) */}
            {data.careerProfile.recommendations && data.careerProfile.recommendations.length > 0 && (
              <div className="recommendations-section">
                <h4 className="rec-title"><FaRocket color="#db2777" /> Gelişim Tavsiyeleri</h4>
                <ul className="rec-list">
                  {data.careerProfile.recommendations.map((rec, i) => (
                    <li key={i} className="rec-item">
                      <span className="rec-bullet">•</span> {rec}
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}

        {/* RPG PERSONA KARTI */}
        {data.workHabits && (
          <div className="card rpg-card" style={{ gridColumn: 'span 3', background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)', color: 'white', padding: '25px', borderRadius: '16px', border: '1px solid #334155' }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '20px' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
                <div style={{ fontSize: '3rem', background: 'rgba(255,255,255,0.05)', padding: '15px', borderRadius: '50%', boxShadow: '0 0 20px rgba(59, 130, 246, 0.2)' }}>
                  {data.workHabits.persona.includes("Night") ? "🦉" :
                    data.workHabits.persona.includes("Early") ? "🌅" :
                      data.workHabits.persona.includes("After") ? "🌇" : "👔"}
                </div>
                <div>
                  <div style={{ fontSize: '0.75rem', letterSpacing: '2px', opacity: 0.7, textTransform: 'uppercase', marginBottom: '5px', color: '#94a3b8' }}>CHARACTER CLASS</div>
                  <h2 style={{ margin: 0, fontSize: '2rem', background: 'linear-gradient(to right, #60a5fa, #c084fc)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', fontWeight: '800' }}>{data.workHabits.persona}</h2>
                </div>
              </div>
            </div>
            <p style={{ fontSize: '1.1rem', opacity: 0.9, marginBottom: '25px', lineHeight: '1.6', color: '#cbd5e1' }}>
              "{data.workHabits.description}"
            </p>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px', background: 'rgba(0,0,0,0.2)', padding: '20px', borderRadius: '12px' }}>
              <div>
                <span style={{ display: 'block', fontSize: '0.8rem', opacity: 0.6, marginBottom: '5px', color: '#94a3b8' }}>⚡ PEAK PERFORMANCE</span>
                <span style={{ fontSize: '1.4rem', fontWeight: 'bold', color: '#facc15' }}>{data.workHabits.peakHours}</span>
              </div>
              <div>
                <span style={{ display: 'block', fontSize: '0.8rem', opacity: 0.6, marginBottom: '5px', color: '#94a3b8' }}>⚔️ WEEKEND GRIND</span>
                <span style={{ fontSize: '1.4rem', fontWeight: 'bold', color: data.workHabits.isWeekendWarrior ? '#f87171' : '#4ade80' }}>
                  {data.workHabits.isWeekendWarrior ? "Warrior (Active)" : "Relaxing (Off)"}
                </span>
              </div>
            </div>
          </div>
        )}

        {/* CHART 1: TECH RADAR */}
        <div className="card chart-card" style={{ gridColumn: 'span 2' }}>
          <div className="card-header">
            <span className="card-title"><FaRocket /> Technology Radar</span>
          </div>

          {chartData.length > 0 ? (
            <div className="chart-container">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={chartData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={5}
                    dataKey="value"
                  >
                    {chartData.map((_entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value: any) => `${Number(value).toFixed(1)}%`} />
                  <Legend verticalAlign="middle" align="right" layout="vertical" iconType="circle" />
                </PieChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div className="no-data">Veri yok.</div>
          )}
        </div>

        {/* CHART 2: HISTORY TREND */}
        {history.length > 1 && (
          <div className="card chart-card" style={{ gridColumn: 'span 3' }}>
            <div className="card-header">
              <span className="card-title"><FaChartLine /> Performance Analysis Trend</span>
            </div>
            <div className="chart-container">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={history} margin={{ top: 20, right: 30, left: 10, bottom: 10 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                  <XAxis dataKey="date" axisLine={false} tickLine={false} tick={{ fill: '#64748b' }} />
                  <YAxis domain={[0, 100]} axisLine={false} tickLine={false} tick={{ fill: '#64748b' }} />
                  <Tooltip contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.1)' }} />
                  <Legend />
                  <Line type="monotone" dataKey="totalScore" stroke="#3b82f6" strokeWidth={4} dot={{ r: 4, fill: '#3b82f6', strokeWidth: 2, stroke: '#fff' }} activeDot={{ r: 8 }} name="Total Score" />
                  <Line type="monotone" dataKey="qualityScore" stroke="#10b981" strokeWidth={3} dot={false} name="Code Quality" />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </div>
        )}

        {/* TOP REPOSITORIES */}
        {data.repositories && data.repositories.length > 0 && (
          <div className="card" style={{ gridColumn: 'span 3' }}>
            <div className="card-header" style={{ marginBottom: '20px' }}>
              <div className="icon-box bg-blue-light text-blue"><FaCode /></div>
              <span className="card-title">📦 Top Repositories ({data.repositories.length} repo)</span>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '12px' }}>
              {[...data.repositories]
                .sort((a, b) => b.stars - a.stars)
                .map((repo, i) => (
                  <div key={i} style={{
                    background: 'linear-gradient(135deg, #f8fafc, #f1f5f9)',
                    border: '1px solid #e2e8f0',
                    borderRadius: '12px',
                    padding: '14px 18px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '8px',
                  }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <span style={{ fontWeight: '700', fontSize: '0.95rem', color: '#1e293b' }}>
                        📁 {repo.name}
                      </span>
                      {repo.stars > 0 && (
                        <span style={{ background: '#fef9c3', color: '#92400e', borderRadius: '8px', padding: '2px 8px', fontSize: '0.8rem', fontWeight: '600' }}>
                          ⭐ {repo.stars}
                        </span>
                      )}
                    </div>
                    <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                      {repo.language && (
                        <span style={{ background: '#dbeafe', color: '#1d4ed8', borderRadius: '20px', padding: '2px 10px', fontSize: '0.75rem', fontWeight: '600' }}>
                          {repo.language}
                        </span>
                      )}
                      {repo.issues > 0 && (
                        <span style={{ background: '#fee2e2', color: '#991b1b', borderRadius: '20px', padding: '2px 10px', fontSize: '0.75rem' }}>
                          🐛 {repo.issues} issue
                        </span>
                      )}
                    </div>
                    <div style={{ fontSize: '0.8rem', color: '#64748b' }}>
                      Score: <b style={{ color: '#7c3aed' }}>{repo.repositoryScore.toFixed(0)}</b>
                    </div>
                  </div>
                ))}
            </div>
          </div>
        )}

        {/* EXPORT BUTONU */}
        <div className="export-section">
          <button className="btn-secondary" onClick={handleDownloadReport}>
            <FaDownload /> Raporu İndir (HTML)
          </button>
        </div>
      </div>
    );
  };

  const renderComparisonAnalysis = () => {
    if (!comparisonData) return null;
    const { developer1, developer2, overallWinner, categoryWinners } = comparisonData;

    return (
      <div className="comparison-container fade-in">
        {/* WINNER BANNER */}
        <div className="winner-banner">
          <FaTrophy size={40} color="#f59e0b" />
          <div>
            <h2>KAZANAN</h2>
            <h1>{overallWinner === "Berabere" ? "DOSTLUK KAZANDI" : overallWinner}</h1>
          </div>
        </div>

        <div className="comparison-grid">
          {/* DEV 1 COLUMN */}
          <div className={`dev-column ${overallWinner === developer1.username ? 'winner-col' : ''}`}>
            <h3>{developer1.username}</h3>
            <div className="big-score">{developer1.totalScore.toFixed(1)}</div>
            <div className="stat-row">
              <span>Kalite:</span> <b>{developer1.qualityScore.toFixed(0)}</b>
              {categoryWinners["Quality"] === developer1.username && <FaMedal color="#f59e0b" />}
            </div>
            <div className="stat-row">
              <span>Aktivite:</span> <b>{developer1.activityScore.toFixed(0)}</b>
              {categoryWinners["Activity"] === developer1.username && <FaMedal color="#f59e0b" />}
            </div>
          </div>

          {/* VS BADGE */}
          <div className="vs-badge">VS</div>

          {/* DEV 2 COLUMN */}
          <div className={`dev-column ${overallWinner === developer2.username ? 'winner-col' : ''}`}>
            <h3>{developer2.username}</h3>
            <div className="big-score">{developer2.totalScore.toFixed(1)}</div>
            <div className="stat-row">
              <span>Kalite:</span> <b>{developer2.qualityScore.toFixed(0)}</b>
              {categoryWinners["Quality"] === developer2.username && <FaMedal color="#f59e0b" />}
            </div>
            <div className="stat-row">
              <span>Aktivite:</span> <b>{developer2.activityScore.toFixed(0)}</b>
              {categoryWinners["Activity"] === developer2.username && <FaMedal color="#f59e0b" />}
            </div>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="app-container">
      {/* HEADER */}
      <header className="app-header">
        <div className="header-content">
          <div className="logo-area">
            <FaGithub size={32} color="#2563eb" />
            <span className="header-title">GitHub Intelligence</span>
          </div>
          <div className="header-badge">v2.1 Enterprise</div>
        </div>
      </header>

      <main className="main-content">

        {/* SEARCH SECTION */}
        <div className="search-section">
          <h1 className="hero-title">Developer Performance <br /> <span className="highlight-text">Analytics</span></h1>

          {/* MODE TOGGLE */}
          <div className="mode-toggle">
            <button
              className={`mode-btn ${!isVersus ? 'active' : ''}`}
              onClick={() => { setIsVersus(false); setData(null); setComparisonData(null); }}
            >
              <FaSearch /> Tekli Analiz
            </button>
            <button
              className={`mode-btn ${isVersus ? 'active' : ''}`}
              onClick={() => { setIsVersus(true); setData(null); setComparisonData(null); }}
            >
              <FaBalanceScale /> Karşılaştırma (VS)
            </button>
          </div>

          <div className="search-box">
            <div className="icon-wrapper"><FaGithub color="#64748b" size={20} /></div>

            <input
              type="text"
              className="input-field"
              placeholder="Kullanıcı Adı 1 (Örn: torvalds)"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleAnalyze()}
            />

            {isVersus && (
              <>
                <div className="divider-vertical"></div>
                <input
                  type="text"
                  className="input-field"
                  placeholder="Kullanıcı Adı 2"
                  value={username2}
                  onChange={(e) => setUsername2(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleAnalyze()}
                />
              </>
            )}

            <button className="btn-primary" onClick={handleAnalyze} disabled={loading}>
              {loading ? <div className="spinner"></div> : isVersus ? 'Kapıştır!' : 'Analiz Et'}
            </button>
          </div>
        </div>

        {/* ERROR */}
        {error && (
          <div className="error-box">
            <FaBug size={20} />
            <span>{error}</span>
          </div>
        )}

        {/* EMPTY STATE */}
        {!data && !comparisonData && !loading && !error && (
          <div className="empty-state">
            <div className="feature-grid">
              <div className="feature-item">
                <div className="feature-icon bg-blue"><FaCode /></div>
                <h3>Teknoloji Radarı</h3>
                <p>Hangi dilleri ne kadar kullandığını gör.</p>
              </div>
              <div className="feature-item">
                <div className="feature-icon bg-green"><FaTrophy /></div>
                <h3>Akıllı Puanlama</h3>
                <p>Kod kalitesi ve aktivite puanı.</p>
              </div>
              <div className="feature-item">
                <div className="feature-icon bg-purple"><FaUserFriends /></div>
                <h3>VS Modu</h3>
                <p>İki geliştiriciyi kıyasla, kazananı gör!</p>
              </div>
            </div>
          </div>
        )}

        {/* SONUÇLAR */}
        {renderSingleAnalysis()}
        {renderComparisonAnalysis()}

      </main>
    </div>
  );
}

export default App;
