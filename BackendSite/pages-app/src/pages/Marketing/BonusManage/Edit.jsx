import React, { useReducer, useEffect, useRef } from "react";
import RadioBox from "./Component/RadioBox";
import { reducer, defActionType, initialState } from "./Edit.reducer";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import Panel from "./Edit.content";

export default props => {
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});
	const [state, dispatch] = useReducer(
		reducer,
		initialState(props.IsAdd, props.Datas, props.SiteList)
	);

	// useEffect(() => {
	// 	let param = {
	// 		Url: "BonusManage/GetTpGameList"
	// 	};
	// 	WebAPI.PostData(param).then(res => {
	// 		if (isMountedRef.current) {
	// 			if (res.data.errorCode === 0) {
	// 				dispatch({
	// 					type: defActionType.GET_TPGAME_CATEGORIES,
	// 					tpGameCategory: res.data.data
	// 				});
	// 			} else {
	// 				alert(res.data.message ?? "failed to get data.");
	// 			}
	// 		}
	// 	});
	// }, []);

	useEffect(() => {
		if(state.tpId !== null){
			let param = {
				Url: "BonusManage/GetTpGameListV2",
				Data: qs.stringify({
					TpId: Number(state.tpId),
					CategoryId: Number(state.categoryId)
				})
			};
			WebAPI.PostData(param).then(res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.GET_TPGAME_CATEGORIES,
							tpGameCategory: res.data.data
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
				}
			});
		}
	}, [state.categoryId]);

	useEffect(() => {
		let param = {
			Url: "BonusManage/GetAllSiteVipLevelList"
		};
		WebAPI.PostData(param).then(res => {
			if (isMountedRef.current) {
				if (res.data.errorCode === 0) {
					dispatch({
						type: defActionType.GET_SITE_VIPLEVEL,
						allSiteVipLevelList: res.data.data
					});
				} else {
					alert(res.data.message ?? "failed to get data.");
				}
			}
		});
	}, []);

	useEffect(() => {
		if (!props.IsAdd) {
			GetPromoCategories(true, -1, props.Datas.walletType);
		}
	}, [props.Datas.walletType, props.IsAdd]);

	const ClickSubmit = () => {
		let checked = true;
		let skipField = props.skipField[state.promotionType] ? props.skipField[state.promotionType] : props.skipField.default;
		for (let key in state) {
			if (key === "specific") {
				for (let c_key in state[key]) {
					if (state.selectedCurrencies.includes(Number(c_key))) {
						for (let f_key in state[key][c_key]) {
							if (skipField.includes(f_key)) continue;
							let column = state[key][c_key][f_key];
							if (f_key === "tpGames" && (state.promotionType == 6 || state.promotionType == 7)) {
								if (Object.keys(state[key][c_key][f_key]).length === 0) {
									alert(
										`${window.i18n[`lbl_Currency${c_key}`]
										} Please open at least one TpGame.` );
									checked = false;
									break;
								}
								for (let t_key in state[key][c_key][f_key]) {
									let tpcolumn = state[key][c_key][f_key][t_key];
									if (Boolean(tpcolumn?.status) && !Boolean(tpcolumn?.percentage)) {
										alert(
											`${window.i18n[`lbl_Currency${c_key}`]
											}'s ${f_key} ${tpcolumn.gameName} percentage does not allow null.`
										);
										checked = false;
										break;
									}
								}
							} else if (
								(column === null ||
								column === undefined ||
								column === "") && f_key !== "affid"
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
				alert("Please select at least one currency.");
				checked = false;
				break;
			}else if (
				key === "forVipLevel" &&
				state.forVipLevel.length === 1 &&
				state.selectedVipLevel.length === 0
			) {
				alert("[For Vip Level] is Open. Please select at least one level.");
				checked = false;
				break;
			}

			if (!checked) {
				break;
			}
		}

		if (checked) {
			props.action.SetPromotion(state);
		}
	};

	let _options_site = props.SiteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});

	const SelectSite = val => {
		let site = props.SiteList.find(s => s.siteId === val);
		dispatch({
			type: defActionType.MAIN_SET_SITE,
			siteId: val,
			currencyList: site.currencyList,
			langList: site.langList
		});
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

	const EditCurrencyAmountValue = (currencyId, value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_AMOUNT_VALUE,
			value,
			dataColumn,
			currencyId
		});
	};

	const EditTpGameCategoryValue = (currencyId, gameName, gameType, value, dataColumn) => {
		dispatch({
			type: defActionType.CURRENCY_SET_TpGame_VALUE,
			value,
			dataColumn,
			currencyId,
			gameName,
			gameType
		});
	};

	const EditOnlyRegisteredValue = (value, dataColumn) => {
		dispatch({ type: defActionType.ONLY_REGISTERED_SET_VALUE, value, dataColumn });
	};

	const EditAutoExtendValue = (value, dataColumn) => {
		dispatch({ type: defActionType.AUTOEXTEND_SET_VALUE, value, dataColumn });
	};

	const EditForVipLevelValue = (value, dataColumn) => {
		dispatch({ type: defActionType.FORVIPLEVEL_SET_VALUE, value, dataColumn });
	};

	const EditSelectedVipLevel = (value, dataColumn) => {
		dispatch({
			type: defActionType.VIPLEVEL_SET_VALUE,
			value,
			dataColumn
		});
	};

	const GetPromoCategories = (isInit, walletType, value) => {
		if (walletType !== value) {
			let param = {
				Url: "BonusManage/GetPromoCategories",
				Data: qs.stringify({
					WalletType: value
				})
			};
			WebAPI.PostData(param).then(res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.GET_PROMO_CATEGORIES,
							value,
							productCategory: res.data.data,
							isInit: isInit
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
				}
			});
		}
	};

	const SelectTpId = (value, dataColumn, mainKey, oIdx) => {
		dispatch({
			type: defActionType.SET_TPID,
			idx: oIdx,
			dataColumn
		});
	};
	return (
		<>
			<div className="filter noBGcolor">
				<RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={SelectSite}
					dataColumn="availableSites"
					Selected={state.siteId}
					Disable={!props.IsAdd}
				/>
			</div>
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
					GetPromoCategories,
					SelectTpId,
					EditCurrencyAmountValue,
					EditTpGameCategoryValue,
					EditOnlyRegisteredValue,
					EditAutoExtendValue,
					EditForVipLevelValue,
					EditSelectedVipLevel
				}}
				siteList={props.SiteList}
				IsAdd={props.IsAdd}
				processing={props.processing}
			/>
		</>
	);
};

