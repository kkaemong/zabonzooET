import {
  ArrowRight,
  ChartCandlestick,
  CirclePlay,
  Gamepad2,
  Landmark,
  Sparkles,
} from 'lucide-react'
import brandLogo from './assets/brand-logo.png'
import playPreview from './assets/play-preview.png'

function App() {
  return (
    <main className="page-shell">
      <section className="hero-panel">
        <div className="hero-copy">
          <span className="eyebrow">Interactive Finance Adventure</span>
          <h1>자본주E.T.</h1>
          <p className="hero-lead">
            시대별 경제 이벤트와 금융 선택을 통과하며, 미래의 자산 궤적을 직접 바꾸는
            스토리형 브라우저 러너 게임.
          </p>

          <div className="hero-actions">
            <a className="primary-link" href="#preview">
              플레이 화면 보기
              <ArrowRight size={18} strokeWidth={2.2} />
            </a>
            <a className="secondary-link" href="#highlights">
              게임 포인트 확인
            </a>
          </div>

          <div className="hero-signals">
            <article className="signal-card">
              <div className="signal-icon">
                <Landmark size={18} />
              </div>
              <span className="signal-label">Stage Flow</span>
              <strong>시대 이동형 금융 러닝</strong>
              <p>1950년대부터 미래까지, 선택에 따라 자산 성장 곡선이 달라집니다.</p>
            </article>
            <article className="signal-card">
              <div className="signal-icon">
                <ChartCandlestick size={18} />
              </div>
              <span className="signal-label">Decision Loop</span>
              <strong>이벤트 반응형 플레이</strong>
              <p>경제 이슈, 퀴즈, 자산 판단이 한 번의 러닝 흐름 안에서 연결됩니다.</p>
            </article>
            <article className="signal-card">
              <div className="signal-icon">
                <CirclePlay size={18} />
              </div>
              <span className="signal-label">Browser Ready</span>
              <strong>설치 없이 즉시 접속</strong>
              <p>브라우저에서 바로 접속해 실제 플레이 감각까지 한 번에 전달합니다.</p>
            </article>
          </div>
        </div>

        <div className="hero-visual">
          <div className="logo-stage">
            <span className="stage-chip">Finance Runner</span>
            <img src={brandLogo} alt="자본주E.T. 로고" />
          </div>

          <div className="loop-card">
            <span className="loop-label">Core Loop</span>
            <p>금융 선택 → 경제 이벤트 반응 → 자산 성장 → 다음 시대로 이동</p>
          </div>
        </div>
      </section>

      <section className="highlight-panel" id="highlights">
        <div className="section-copy">
          <span className="eyebrow">Game Overview</span>
          <h2>한 번의 러닝 안에서 자산의 방향이 달라지는 구조</h2>
          <p>
            단순 주행이 아니라, 시대별 금융 맥락과 선택의 결과를 플레이 템포 안에 녹인
            구조를 지향했습니다.
          </p>
        </div>

        <div className="highlight-grid">
          <article className="highlight-card">
            <Sparkles className="highlight-icon" size={20} />
            <strong>경제 이벤트 중심 전개</strong>
            <p>시장 변화와 사회적 이슈가 곧 플레이 변수로 이어지도록 설계했습니다.</p>
          </article>
          <article className="highlight-card">
            <Gamepad2 className="highlight-icon" size={20} />
            <strong>퀴즈와 선택의 연결</strong>
            <p>학습 정보가 분리된 보조 콘텐츠가 아니라, 진행 흐름과 직접 맞물립니다.</p>
          </article>
          <article className="highlight-card">
            <ChartCandlestick className="highlight-icon" size={20} />
            <strong>결과가 남는 성장 구조</strong>
            <p>플레이 결과가 자산 변화와 다음 선택의 맥락으로 이어지는 순환을 만듭니다.</p>
          </article>
        </div>
      </section>

      <section className="preview-panel" id="preview">
        <div className="section-copy preview-copy">
          <span className="eyebrow">Play Experience</span>
          <h2>소개용 샘플이 아니라, 바로 체감되는 브라우저 플레이 화면</h2>
          <p>
            실제 브라우저 빌드 기준의 장면 톤과 UI 구성을 중심으로, 제품처럼 보이도록
            플레이 섹션을 정리했습니다.
          </p>
        </div>

        <div className="preview-frame">
          <div className="preview-frame-head">
            <span className="preview-pill">Play Session</span>
            <span className="preview-note">Chrome 기반 브라우저에서 가장 안정적으로 감상할 수 있습니다.</span>
          </div>

          <div className="preview-shot">
            <img src={playPreview} alt="자본주E.T. 브라우저 플레이 미리보기" />
          </div>

          <div className="preview-foot">
            <span>In-Browser Experience</span>
            <strong>자본주E.T.</strong>
          </div>
        </div>
      </section>
    </main>
  )
}

export default App
