
import axios from 'axios';
import type { DeveloperDto, AnalysisHistoryDto, DeveloperComparison } from '../models/Developer';

// Production (Canlı) Modu: API ve Frontend aynı domainde çalışır.
// Bu yüzden tam adres yerine sadece "/api" kullanmak yeterlidir.
const API_BASE_URL = '/api';

export const DeveloperService = {
    // 1. Yeni Analiz Başlat
    async analyzeDeveloper(username: string): Promise<DeveloperDto> {
        const response = await axios.post<DeveloperDto>(`${API_BASE_URL}/Developers/${username}/analyze`);
        return response.data;
    },

    // 2. Var Olan Sonucu Getir (Okuma)
    async getAnalysisResult(username: string): Promise<DeveloperDto> {
        const response = await axios.get<DeveloperDto>(`${API_BASE_URL}/Developers/${username}`);
        return response.data;
    },

    // 3. Tarihçe Getir
    async getHistory(username: string): Promise<AnalysisHistoryDto[]> {
        const response = await axios.get<AnalysisHistoryDto[]>(`${API_BASE_URL}/developers/${username}/history`);
        return response.data;
    },

    // 4. Yeni: Karşılaştırma Analizi (Versus Mode)
    async compareDevelopers(user1: string, user2: string): Promise<DeveloperComparison> {
        // İki parametreyi query string olarak gönderiyoruz
        const response = await axios.get<DeveloperComparison>(`${API_BASE_URL}/developers/compare`, {
            params: { user1, user2 }
        });
        return response.data;
    },

    // 4. Rapor İndir (HTML)
    getExportUrl(username: string): string {
        return `${API_BASE_URL}/Developers/${username}/export`;
    }
};
