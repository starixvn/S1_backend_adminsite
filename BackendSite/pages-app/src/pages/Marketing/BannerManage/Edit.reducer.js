export const defActionType = {
	MAIN_SET_VALUE: "MAIN_SET_VALUE",
	MAIN_SET_CURRENCY_CHECKBOX: "MAIN_SET_CURRENCY_CHECKBOX",
	CURRENCY_SET_VALUE: "CURRENCY_SET_VALUE",
	CURRENCY_LANG_SET_VALUE: "CURRENCY_LANG_SET_VALUE",
	CURRENCY_SET_LANG_CHECKBOX: "CURRENCY_SET_LANG_CHECKBOX",
	SPORTS_LEAGUES_SET_LIST: "SPORTS_LEAGUES_SET_LIST",
	SPORTS_TEAMS_SET_LIST: "SPORTS_TEAMS_SET_LIST",
	MAIN_SET_SPORTS_TEAMS_VALUE: "MAIN_SET_SPORTS_TEAMS_VALUE",
};

export const initialState = (IsAdd, datas, setSite, setCurrencyList, setLangList) => {
	if (IsAdd) {
		let _specific = SetSpecific(
			setCurrencyList,
			setLangList,
			null
		);
		return {
			siteId: setSite,
			bannerName: "",
			bannerType: 1,
			// status: 0,
			sort: "",
			actionMode: "",
			showStatus: "",
			startTime: new Date(),
			endTime: new Date(),
			bonusCode: "",
			matchId: "",
			tpId: "",
			gameId: "",
			urlRouter: "",
			sportBattleDateTime: new Date(),
			sportCategoryId: 1,
			sportLeaguesId: 1,
			sportTeamsId: { "home": 1, "away": 1 },
			sportsLeaguesList: [],
			sportsTeamsList: [],
			selectedCurrencies: [],
			currentTab: null,
			specific: _specific,
		};
	} else {
		let _specific = SetSpecific(
			setCurrencyList,
			setLangList,
			datas.specific
		);
		let _selectedCurrencies = datas.specific.map(s => {
			return s.currencyId;
		});
		return {
			siteId: datas.siteId,
			bannerName: datas.bannerName,
			bannerType: datas.bannerType,
			// status: datas.status,
			sort: datas.sort,
			actionMode: datas.actionMode,
			showStatus: datas.showStatus,
			startTime: datas.startTime,
			endTime: datas.endTime,
			bonusCode: datas.bonusCode,
			matchId: datas.matchId,
			tpId: datas.tpId,
			gameId: datas.gameId,
			urlRouter: datas.urlRouter,
			sportBattleDateTime: datas.sportBattleDateTime,
			sportCategoryId: 1,
			sportLeaguesId: datas.sportLeaguesId,
			sportTeamsId: (datas.sportTeamsId[0]) ? datas.sportTeamsId[0] : {},
			sportsLeaguesList: [],
			sportsTeamsList: [],
			currentTab: _selectedCurrencies[0],
			selectedCurrencies: _selectedCurrencies,
			specific: _specific,
		};
	}
};

const initLang = {
	bannerLink: "",
	bannerPCUrl: "",
	bannerMobileUrl: ""
};

const initSpecific = {
	status: 1,
	selectedLang: [],
	currentLang: null,
	lang: {
		en: { ...initLang }
	},
};

const SetSpecific = (currencyList, langList, data) => {
	let _specific = {};
	currencyList.forEach(currency => {
		_specific[currency] = { ...initSpecific };
		let lang = {};
		langList.forEach(l => {
			lang = {
				...lang,
				[l]: { ...initLang }
			};
		});
		_specific[currency].lang = lang;
	});

	if (data !== null) {
		data.forEach(d => {
			let _newData = {};
			let lang = JSON.parse(d.details ?? "{}");
			d["lang"] = lang;
			d["selectedLang"] = Object.keys(lang);
			langList
				.filter(l => {
					return !d["selectedLang"].includes(l);
				})
				.forEach(l => {
					d.lang = {
						...d.lang,
						[l]: { ...initLang }
					};
				});
			d["currentLang"] = d["selectedLang"][0] ?? "";
			Object.keys(initSpecific).forEach(k => {
				_newData[k] = d[k] ?? "";
			});
			_specific[d.currencyId] = _newData;
		});
	}
	return _specific;
};

export function reducer(state, action) {
	switch (action.type) {
		case defActionType.MAIN_SET_VALUE:
			return {
				...state,
				[action.dataColumn]: action.value
			};

		case defActionType.MAIN_SET_CURRENCY_CHECKBOX:
			let isRemove = state[action.dataColumn].includes(action.value);
			let newCurrencies = isRemove
				? state[action.dataColumn].filter(v => v !== action.value)
				: [...state[action.dataColumn], action.value];

			let currentTab = state.currentTab;
			if (newCurrencies.length === 0) {
				currentTab = null;
			} else if (currentTab === null) {
				currentTab = newCurrencies[0];
			}

			if (isRemove && !newCurrencies.includes(currentTab)) {
				currentTab =
					newCurrencies.length === 0 ? null : newCurrencies[0];
			}

			return {
				...state,
				[action.dataColumn]: newCurrencies,
				currentTab: currentTab
			};

		case defActionType.CURRENCY_SET_VALUE:
			return {
				...state,
				specific: {
					...state.specific,
					[action.currencyId]: {
						...state.specific[action.currencyId],
						[action.dataColumn]: action.value
					}
				}
			};

		case defActionType.CURRENCY_LANG_SET_VALUE:
			let selCurrency = state.currentTab;
			let selLang = state.specific[selCurrency].currentLang;
			return {
				...state,
				specific: {
					...state.specific,
					[selCurrency]: {
						...state.specific[selCurrency],
						lang: {
							...state.specific[selCurrency].lang,
							[selLang]: {
								...state.specific[selCurrency].lang[selLang],
								[action.dataColumn]: action.value
							}
						}
					}
				}
			};

		case defActionType.CURRENCY_SET_LANG_CHECKBOX:
			let selCurr = state.currentTab;
			let selLangs = state.specific[selCurr].selectedLang;
			let isLangRemove = selLangs.includes(action.value);
			let newLangs = isLangRemove
				? selLangs.filter(v => v !== action.value)
				: [...selLangs, action.value];

			let currLang = state.specific[selCurr].currentLang;
			if (newLangs.length === 0) {
				currLang = null;
			} else if (currLang === null || currLang === "") {
				currLang = newLangs[0];
			}

			if (isLangRemove && !newLangs.includes(currLang)) {
				currLang = newLangs.length === 0 ? null : newLangs[0];
			}

			return {
				...state,
				specific: {
					...state.specific,
					[selCurr]: {
						...state.specific[selCurr],
						selectedLang: newLangs,
						currentLang: currLang
					}
				}
			};

		case defActionType.SPORTS_LEAGUES_SET_LIST:
			let sportsLeaguesList = action.sportsLeaguesList.map(s => {
				return {
					value: s.id,
					displayText: s.name,
				};
			});
			return {
				...state,
				sportsLeaguesList: sportsLeaguesList
			};

		case defActionType.SPORTS_TEAMS_SET_LIST:
			let sportsTeamsList = action.sportsTeamsList.map(s => {
				return {
					value: s.id,
					displayText: s.name,
				};
			});
			return {
				...state,
				[action.dataColumn]: action.value,
				sportsTeamsList: sportsTeamsList,
				sportTeamsId: {
					...state.sportTeamsId,
					"home": action.action == "default" ? state.sportTeamsId.home : sportsTeamsList[0].value,
					"away": action.action == "default" ? state.sportTeamsId.away : sportsTeamsList[0].value
				}
			};

		case defActionType.MAIN_SET_SPORTS_TEAMS_VALUE:
			return {
				...state,
				sportTeamsId: {
					...state.sportTeamsId,
					[action.dataColumn]: action.value
				}
			};

		default:
			throw new Error();
	}
}
