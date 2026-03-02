
// Bu dosya, Backend'deki C# DeveloperDto.cs'nin karşılığıdır.
// Veri tiplerinin tutarlı olduğundan emin olmalıyız.

export interface AnalysisHistoryDto {
    date: string;
    totalScore: number;
    qualityScore: number;
    activityScore: number;
}

export interface LanguageDistribution {
    [language: string]: number; // "C#": 60, "Python": 40
}

export interface RepositoryDto {
    name: string;
    language: string | null;
    stars: number;
    issues: number;
    repositoryScore: number;
}

export interface DeveloperDto {
    id: string; // GUID --> string
    username: string;
    name: string;
    email: string | null;
    bio: string | null;

    totalScore: number;
    seniorityLevel: string; // "Junior", "Senior" etc.
    activityScore: number;
    qualityScore: number;
    popularityScore: number;

    lastAnalysisDate: string; // Date --> string (ISO format)

    languageDistribution: LanguageDistribution | null;
    careerProfile?: CareerProfile;
    workHabits?: WorkHabits; // YENİ
    repositories: RepositoryDto[];
}

export interface CareerProfile {
    title: string;
    description: string;
    suitableRoles: string[];
    recommendations: string[];
}

export interface WorkHabits {
    persona: string;
    description: string;
    peakHours: string;
    isWeekendWarrior: boolean;
}

export interface DeveloperComparison {
    developer1: DeveloperDto;
    developer2: DeveloperDto;
    overallWinner: string;
    scoreDifference: number;
    categoryWinners: Record<string, string>; // { "TotalScore": "username1", "Quality": "username2" }
}
