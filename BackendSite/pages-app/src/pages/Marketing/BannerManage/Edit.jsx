import React, { useReducer, useEffect, useRef } from "react";
import { reducer, defActionType, initialState } from "./Edit.reducer";
import Panel from "./Edit.content";
import WebAPI from "../../../module/WebAPI";

export default props => {
	moment.tz.setDefault("Asia/Taipei");
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let SportLeaguesListParam = {
				Url: "Sports/GetSportLeaguesList",
				Data: {
					SportCategoryId: 1
				}
			};
			console.log(props.Datas.sportLeaguesId);
			WebAPI.PostData(SportLeaguesListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SPORTS_LEAGUES_SET_LIST,
								sportsLeaguesList: res.data.data
							});
							GetSportTeamsList("default", props.Datas.sportLeaguesId ? props.Datas.sportLeaguesId : 1, "sportLeaguesId");
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						props.action.setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						props.action.setProcessing(false);
					}
				}
			);
		}
	}, []);

	const [state, dispatch] = useReducer(
		reducer,
		initialState(props.IsAdd, props.Datas, props.SetSite, props.SetCurrencyList, props.SetLangList)
	);

	const GetSportTeamsList = (action, leaguesId, dataColumn) => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let SportTeamsListParam = {
				Url: "Sports/GetSportTeamsList",
				Data: {
					SportLeaguesId: Number(leaguesId)
				}
			};
			WebAPI.PostData(SportTeamsListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SPORTS_TEAMS_SET_LIST,
								value: leaguesId,
								dataColumn,
								sportsTeamsList: res.data.data,
								action: action
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						props.action.setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						props.action.setProcessing(false);
					}
				}
			);
		}
	}

	const ClickSubmit = () => {
		let checked = true;
		let skipField = props.skipField[state.bannerType] ? props.skipField[state.bannerType] : props.skipField.default;

		for (let key in state) {
			if (key === "specific") {
				for (let c_key in state[key]) {
					if (state.selectedCurrencies.includes(Number(c_key))) {
						for (let f_key in state[key][c_key]) {
							if (skipField.includes(f_key)) continue;
							let column = state[key][c_key][f_key];
							if (
								column === null ||
								column === undefined ||
								column === ""
							) {
								alert(
									`${window.i18n[`lbl_Currency${c_key}`]
									}'s ${f_key} does not allow null.`
								);
								checked = false;
								break;
							}
						}
					}

					if (!checked) {
						break;
					}
				}
			} else if (
				state[key] === null ||
				state[key] === undefined ||
				state[key] === ""
			) {
				if (skipField.includes(key)) continue;
				alert(`${key} does not allow null.`);
				checked = false;
				break;
			} else if (
				key === "selectedCurrencies" &&
				state[key].length === 0
			) {
				if (skipField.includes(key)) continue;
				alert("Please select at least one currency.");
				checked = false;
				break;
			} else if (
				key === "sportTeamsId" &&
				state[key].home == state[key].away
			) {
				if (skipField.includes(key)) continue;
				alert("Please choose a different team.");
				checked = false;
				break;
			}

			if (!checked) {
				break;
			}
		}
		if (checked) {
			props.action.SetBanner(state);
		}
	};

	const EditMainValue = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_VALUE, value, dataColumn });
	};

	const EditCurrencyCheckBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.MAIN_SET_CURRENCY_CHECKBOX,
			value,
			dataColumn
		});
	};

	const EditCurrencyPanel = (currencyId, value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_VALUE,
			value,
			dataColumn,
			currencyId
		});
	};

	const EditCurrencyLangPanelValue = (value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_LANG_SET_VALUE,
			value,
			dataColumn
		});
	};

	const EditCurrencyLangCheckBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_LANG_CHECKBOX,
			value,
			dataColumn
		});
	};

	const EditSportsTeamsValue = (value, dataColumn) => {
		dispatch({ type: defActionType.MAIN_SET_SPORTS_TEAMS_VALUE, value, dataColumn });
	};

	return (
		<Panel
			dispatch={dispatch}
			state={state}
			action={{
				ClickSubmit,
				EditMainValue,
				EditCurrencyCheckBox,
				EditCurrencyPanel,
				ClickBack: props.action.ClickBack,
				EditCurrencyLangPanelValue,
				EditCurrencyLangCheckBox,
				GetSportTeamsList,
				EditSportsTeamsValue
			}}
			IsAdd={props.IsAdd}
			processing={props.processing}
		/>
	);
};

