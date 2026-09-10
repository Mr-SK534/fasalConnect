import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import GrassCanvas from './GrassCanvas';
import LanguageSelector from '../components/common/LanguageSelector';

// Inline SVG Icons
const FreshProduceIcon = ({ className = "w-5 h-5" }) => (
  <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 19l9 2-2-9a9.99 9.99 0 00-7.3-7.3 9.99 9.99 0 00-7.3 7.3L2 21l9-2zm0 0v-8" />
  </svg>
);

const DirectBuyersIcon = ({ className = "w-5 h-5" }) => (
  <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 100 4 2 2 0 000-4z" />
  </svg>
);

const BetterPricesIcon = ({ className = "w-5 h-5" }) => (
  <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
  </svg>
);

const CommunitiesIcon = ({ className = "w-5 h-5" }) => (
  <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 21a9 9 0 009-9c0-4.97-4.03-9-9-9s-9 4.03-9 9a9 9 0 009 9z" />
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 21V11m0 0c2.5 0 5-2.5 5-5m-5 5c-2.5 0-5-2.5-5-5" />
  </svg>
);

const ArrowRightIcon = ({ className = "w-4 h-4" }) => (
  <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M14 5l7 7m0 0l-7 7m7-7H3" />
  </svg>
);

export default function Home() {
  const { t } = useTranslation();

  return (
    <div className="relative min-h-screen w-full bg-[#8aceff] font-sans text-gray-800 overflow-x-hidden flex flex-col justify-between">

      {/* BACKGROUND MOVING CLOUDS LAYER */}
      <div className="absolute inset-0 pointer-events-none z-0 overflow-hidden">
        <div 
          className="absolute top-10 w-[500px] h-[180px] bg-white/40 rounded-full blur-2xl animate-cloud-slow"
          style={{ left: '-10%' }}
        />
        <div 
          className="absolute top-24 w-[380px] h-[140px] bg-white/50 rounded-full blur-xl animate-cloud-mid"
          style={{ left: '-25%' }}
        />
        <div 
          className="absolute top-4 w-[600px] h-[200px] bg-white/30 rounded-full blur-3xl animate-cloud-fast"
          style={{ left: '-15%' }}
        />
      </div>

      {/* Header Container */}
      <header className="relative z-30 w-full px-6 sm:px-10 lg:px-12 pt-4 pb-2 flex items-center justify-between">
        <div className="flex items-center space-x-2.5">
          <div className="text-emerald-700 bg-white/90 p-1.5 rounded-xl border border-emerald-100 shadow-sm backdrop-blur-sm">
            <FreshProduceIcon className="w-7 h-7" />
          </div>
          <div>
            <h1 className="text-xl font-black text-emerald-950 tracking-tight leading-none">
              Fasal<span className="text-emerald-600">Connect</span>
            </h1>
            <p className="text-[10px] text-gray-700 font-bold mt-0.5">
              {t('header.tagline', 'From Our Fields to a Brighter Tomorrow')}
            </p>
          </div>
        </div>

        {/* Action Controls & Language Selector */}
        <div className="flex items-center space-x-3 sm:space-x-4 ml-auto">
          <LanguageSelector />

          <Link 
            to="/login" 
            className="text-xs font-bold text-gray-700 bg-white/90 border border-gray-200 px-4 sm:px-5 py-2.5 rounded-xl 
                       shadow-[0_3px_0_0_#e5e7eb] hover:shadow-[0_1px_0_0_#e5e7eb] hover:translate-y-[2px] 
                       active:shadow-none active:translate-y-[3px] transition-all whitespace-nowrap backdrop-blur-sm"
          >
            {t('nav.signIn', 'Sign in')}
          </Link>

          <Link 
            to="/register" 
            className="text-xs font-bold text-white bg-emerald-600 border border-emerald-700 px-4 sm:px-5 py-2.5 rounded-xl 
                       shadow-[0_3px_0_0_#047857] hover:shadow-[0_1px_0_0_#047857] hover:translate-y-[2px] 
                       active:shadow-none active:translate-y-[3px] transition-all whitespace-nowrap"
          >
            {t('nav.getStarted', 'Get started')}
          </Link>
        </div>
      </header>

      {/* Main Content Layout Grid */}
      <main className="relative z-20 max-w-7xl w-full mx-auto px-6 sm:px-10 lg:px-12 grid grid-cols-1 lg:grid-cols-12 items-center flex-1 my-auto h-full">
        
        {/* Left Hero Content */}
        <div className="lg:col-span-5 space-y-4 max-w-lg z-30">
          <div>
            <h2 className="text-3xl sm:text-4xl lg:text-5xl font-black text-slate-900 leading-[1.08] tracking-tight">
              {t('hero.connecting', 'Connecting')} <br />
              <span className="text-emerald-600">{t('hero.farmers', 'Farmers')}</span> {t('hero.to', 'to')} <br />
              {t('hero.betterTomorrow', 'a Better Tomorrow')}
            </h2>
            <div className="w-16 h-1 bg-emerald-500 rounded-full mt-2.5"></div>
          </div>

          <p className="text-gray-800 text-sm sm:text-base font-bold leading-normal">
            {t('hero.subheading', 'A modern marketplace for fresh produce, fair prices, and stronger farming communities.')}
          </p>

          {/* 3D Feature Cards Grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-2.5 py-1">
            <div className="flex flex-col items-center justify-center text-center p-2.5 rounded-xl bg-white/95 border-2 border-emerald-200 
                            shadow-[0_4px_0_0_#a7f3d0] hover:shadow-[0_2px_0_0_#a7f3d0] hover:translate-y-[2px] active:shadow-none active:translate-y-[4px] 
                            transition-all cursor-pointer backdrop-blur-sm">
              <div className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 mb-1">
                <FreshProduceIcon className="w-5 h-5" />
              </div>
              <span className="text-xs sm:text-sm font-extrabold text-gray-900 leading-tight">
                {t('features.freshProduce', 'Fresh Produce')}
              </span>
            </div>

            <div className="flex flex-col items-center justify-center text-center p-2.5 rounded-xl bg-white/95 border-2 border-emerald-200 
                            shadow-[0_4px_0_0_#a7f3d0] hover:shadow-[0_2px_0_0_#a7f3d0] hover:translate-y-[2px] active:shadow-none active:translate-y-[4px] 
                            transition-all cursor-pointer backdrop-blur-sm">
              <div className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 mb-1">
                <DirectBuyersIcon className="w-5 h-5" />
              </div>
              <span className="text-xs sm:text-sm font-extrabold text-gray-900 leading-tight">
                {t('features.directBuyers', 'Direct Buyers')}
              </span>
            </div>

            <div className="flex flex-col items-center justify-center text-center p-2.5 rounded-xl bg-white/95 border-2 border-emerald-200 
                            shadow-[0_4px_0_0_#a7f3d0] hover:shadow-[0_2px_0_0_#a7f3d0] hover:translate-y-[2px] active:shadow-none active:translate-y-[4px] 
                            transition-all cursor-pointer backdrop-blur-sm">
              <div className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 mb-1">
                <BetterPricesIcon className="w-5 h-5" />
              </div>
              <span className="text-xs sm:text-sm font-extrabold text-gray-900 leading-tight">
                {t('features.betterPrices', 'Better Prices')}
              </span>
            </div>

            <div className="flex flex-col items-center justify-center text-center p-2.5 rounded-xl bg-white/95 border-2 border-emerald-200 
                            shadow-[0_4px_0_0_#a7f3d0] hover:shadow-[0_2px_0_0_#a7f3d0] hover:translate-y-[2px] active:shadow-none active:translate-y-[4px] 
                            transition-all cursor-pointer backdrop-blur-sm">
              <div className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 mb-1">
                <CommunitiesIcon className="w-5 h-5" />
              </div>
              <span className="text-xs sm:text-sm font-extrabold text-gray-900 leading-tight">
                {t('features.strongerCommunities', 'Stronger Communities')}
              </span>
            </div>
          </div>

      {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 pt-1">
            <Link 
              to="/register" 
              className="flex items-center justify-center space-x-2 bg-emerald-600 hover:bg-emerald-500 text-white text-xs sm:text-sm font-extrabold px-5 py-3 rounded-xl 
                         border-2 border-emerald-700 shadow-[0_4px_0_0_#047857] hover:shadow-[0_2px_0_0_#047857] 
                         hover:translate-y-[2px] active:shadow-none active:translate-y-[4px] transition-all w-full sm:w-auto"
            >
              <span>{t('cta.shopProducts', 'Shop Fresh Products')}</span>
              <ArrowRightIcon className="w-4 h-4" />
            </Link>

            <Link 
              to="/register" 
              className="flex items-center justify-center bg-white hover:bg-emerald-50 text-emerald-900 text-xs sm:text-sm font-extrabold px-5 py-3 rounded-xl 
                         border-2 border-emerald-300 shadow-[0_4px_0_0_#a7f3d0] hover:shadow-[0_2px_0_0_#a7f3d0] 
                         hover:translate-y-[2px] active:shadow-none active:translate-y-[4px] transition-all w-full sm:w-auto text-center"
            >
              {t('cta.joinFarmer', 'Join as a Farmer')}
            </Link>
          </div>

          <div className="flex items-center space-x-1.5 text-emerald-950 text-xs font-extrabold italic">
            <FreshProduceIcon className="w-3.5 h-3.5 text-emerald-700" />
            <span>{t('footer.motto', 'Together for a Greener Tomorrow')}</span>
          </div>
        </div>

        {/* Right Mascot Column */}
        <div className="hidden lg:flex lg:col-span-7 h-full items-end justify-center relative pointer-events-none z-10">
          <img 
            src="/mascot.png" 
            alt="FasalConnect Farmer Mascot" 
            className="h-[90vh] max-w-none object-contain object-bottom translate-y-10 filter drop-shadow-lg"
          />
        </div>

      </main>

      {/* TOP-LEVEL GRASS OVERLAY */}
      <GrassCanvas />

    </div>
  );
}
