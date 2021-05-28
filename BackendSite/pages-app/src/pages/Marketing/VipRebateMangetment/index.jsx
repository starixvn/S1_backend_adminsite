import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import RadioBox from "./Component/RadioBox";
import Panel from "./index.content";
import {
	defActionType,
	reducer,
	initState
} from "./index.reducer";

const App = () => {
	const [state, dispatch] = useReducer(reducer, initState);
	const [processing, setProcessing] = useState(false);
	const [processArea, setProcessArea] = useState(null);
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		let siteParam = {
			Url: "Common/AdminUserSiteList"
		};
		WebAPI.PostData(siteParam).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.SET_INIT_STATE,
							state: "initSiteList",
							data: res.data.data
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
					setProcessing(false);
				}
			},
			err => {
				if (isMountedRef.current) {
					alert("API Server Error.");
					setProcessing(false);
				}
			}
		);
		let tpCategoryParam = {
			Url: "RebateManage/GetTPCategoryList"
		};
		WebAPI.PostData(tpCategoryParam).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.SET_INIT_STATE,
							state: "initTpCategoryList",
							data: res.data.data
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
					setProcessing(false);
				}
			},
			err => {
				if (isMountedRef.current) {
					alert("API Server Error.");
					setProcessing(false);
				}
			}
		);
	}, []);

	const EditMainValue = (value, dataColumn) => {
		dispatch({ type: defActionType.SET_INIT_STATE, state: value, data: dataColumn });
	};
	const SetData = (value, dataColumn) => {
		dispatch({ type: defActionType.SET_DATA, state: dataColumn, data: value });
	}
	const SetSite = siteId => {
		dispatch({
			type: defActionType.GET_SITE_INFO,
			site: siteId,
			siteCurrencyList: state.initSiteList.find(s => s.siteId === siteId).iCurrencyList
		});
		GetSiteVipInfo(siteId);
		GetSiteRebatePeriodInfo(siteId);
	}

	const GetSiteVipInfo = siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let vipParam = {
				Url: "RebateManage/GetVipLevel",
				Data: {
					SiteId: siteId
				}
			};
			WebAPI.PostData(vipParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_INIT_STATE,
								state: "siteVipList",
								data: res.data.data,
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
					}
				}
			);
		}
	}

	const GetSiteRebatePeriodInfo = siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
				setProcessArea("RebatePeriod");
			}
			let rebatePeriodParam = {
				Url: "RebateManage/GetRebatePeriod",
				Data: {
					SiteId: siteId
				}
			};
			WebAPI.PostData(rebatePeriodParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_INIT_STATE,
								state: "setRebatePeriodData",
								data: res.data.data,
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
						setProcessArea(null);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
						setProcessArea(null);
					}
				}
			);
		}
	}

	const GetRebateInfo = currencyId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
				setProcessArea("RebateInfo");
			}
			let rebatePeriodParam = {
				Url: "RebateManage/GetRebateInfo",
				Data: {
					SiteId: state.setSiteData,
					CurrencyId: currencyId
				}
			};
			WebAPI.PostData(rebatePeriodParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.GET_REBATE_INFO,
								setCurrencyTab: currencyId,
								setCurrencyData: res.data.data
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
						setProcessArea(null);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
						setProcessArea(null);
					}
				}
			);
		}
	}

	const GetTpGameTypeListbyCategory = categoryId => {
		let categoryData = state.setCurrencyData.categoryDataList.find(s => s.categoryId === categoryId);
		let categoryPayoutData = state.setCurrencyData.payoutLimitList.find(s => s.categoryId === categoryId);
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
				setProcessArea("TpGameTypeList");
			}
			let rebatePeriodParam = {
				Url: "RebateManage/GetTpGameTypeListbyCategory",
				Data: {
					SiteId: state.setSiteData,
					CategoryId: categoryId
				}
			};
			WebAPI.PostData(rebatePeriodParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.GET_REBATE_INFO_BY_TPCATEGORY,
								setTpCategoryTab: categoryId,
								setCategoryPayoutData: categoryPayoutData,
								setCategoryData: categoryData,
								settpGameTypeListbyCategory: res.data.data
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
						setProcessArea(null);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
						setProcessArea(null);
					}
				}
			);
		}
	}

	const GetRebateInfoByGameType = (CurrencyId, GameType) => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
				setProcessArea("RebateInfoByGameType");
			}
			let rebatePeriodParam = {
				Url: "RebateManage/GetRebateInfoByGameType",
				Data: {
					SiteId: state.setSiteData,
					CurrencyId: CurrencyId,
					GameType: GameType
				}
			};
			WebAPI.PostData(rebatePeriodParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.GET_REBATE_INFO_BY_GAME_TYPE,
								setTpGameTypeTab: GameType,
								setGameTypeData: res.data.data
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
						setProcessArea(null);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
						setProcessArea(null);
					}
				}
			);
		}
	}

	const SetRebatePeriod = () => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			if (state.setData.periodType === 2) {
				if (
					state.setData.periodWeek === null ||
					state.setData.periodWeek === undefined ||
					state.setData.periodWeek === "" ||
					state.setData.periodWeek === 0) {
					alert(
						`Week Option does not allow null.`
					);
					setProcessing(false);
					return false;
				}
			} else if (state.setData.periodType === 3) {
				if (
					state.setData.periodMonth === null ||
					state.setData.periodMonth === undefined ||
					state.setData.periodMonth === "" ||
					state.setData.periodMonth === 0) {
					alert(
						`Date does not allow null.`
					);
					setProcessing(false);
					return false;
				}
			} else if (state.setData.periodType === null ||
				state.setData.periodType === undefined ||
				state.setData.periodType === "") {
				alert(
					`Period Type does not allow null.`
				);
				setProcessing(false);
				return false;
			}

			if (state.setData.redeemHours === null ||
				state.setData.redeemHours === undefined ||
				state.setData.redeemHours === "") {
				alert(
					`Redeem Hours does not allow null.`
				);
				setProcessing(false);
				return false;
			}

			let rebatePeriodParam = {
				Url: "RebateManage/SetRebatePeriod",
				Data: {
					SiteId: Number(state.setSiteData),
					PeriodType: Number(state.setData.periodType),
					PeriodDay: Number(state.setData.periodType) === 2 ? Number(state.setData.periodWeek) : Number(state.setData.periodType) === 3 ? Number(state.setData.periodMonth) : 0,
					RedeemHours: Number(state.setData.redeemHours)
				}
			};
			WebAPI.PostData(rebatePeriodParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							GetSiteRebatePeriodInfo(Number(state.setSiteData));
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
					}
				}
			);
		}
	}

	const SetRebatePayoutLimit = () => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let rebatePeriodLimitParam = {
				Url: "RebateManage/SetRebatePayoutLimit",
				Data: {
					SiteId: Number(state.setSiteData),
					CurrencyId: Number(state.currencyTab),
					CategoryId: Number(state.tpCategoryTab),
					MaxPayout: state.setData.maxPayout ? Number(state.setData.maxPayout) : 0,
					MinPayout: state.setData.minPayout ? Number(state.setData.minPayout) : 0
				}
			};
			WebAPI.PostData(rebatePeriodLimitParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							GetRebateInfo(Number(state.currencyTab));
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
					}
				}
			);
		}
	}

	const SetRebateCategory = level => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let rebateCategoryParam = {
				Url: "RebateManage/SetRebateCategory",
				Data: {
					SiteId: Number(state.setSiteData),
					CurrencyId: Number(state.currencyTab),
					CategoryId: Number(state.tpCategoryTab),
					level: level ? Number(level) : 0,
					percentage: state.setData.categoryDataLevel ? Number(state.setData.categoryDataLevel) : 0
				}
			};

			WebAPI.PostData(rebateCategoryParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							GetRebateInfo(Number(state.currencyTab));
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
					}
				}
			);
		}
	}

	const SetRebateGameType = level => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let rebateGameTypeParam = {
				Url: "RebateManage/SetRebateGameType",
				Data: {
					SiteId: Number(state.setSiteData),
					CurrencyId: Number(state.currencyTab),
					GameType: Number(state.tpGameTypeTab),
					level: level ? Number(level) : 0,
					percentage: state.setData.tpGameTypeLevel ? Number(state.setData.tpGameTypeLevel) : 0
				}
			};
			WebAPI.PostData(rebateGameTypeParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							GetRebateInfoByGameType(Number(state.currencyTab), Number(state.tpGameTypeTab))
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
					}
				}
			);
		}
	}


	const HandleEdit = (dataColumn, idx, data = "") => {
		let i = dataColumn + idx;
		let isEdit = state.initEdit[i] === true ? false : true;
		dispatch({ type: defActionType.SET_EDIT, idx: i, val: isEdit, originalData: data });
	};

	let _options_site = state.initSiteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});
	// console.log("After state", state);
	//console.log("Edit state", state.setData);
	return (
		<div className="container-fluid">
			<h4 className="page-name">Vip Rebate Mangetment</h4>
			<div className="filter" style={{ position: "sticky", top: 0, left: 0, zIndex: 999, width: "100%", height: "90px" }}>
				<RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={SetSite}
					dataColumn="availableSites"
					Selected={state.setSiteData}
				/>
			</div >
			<div className="filter noBGcolor"></div>
			{
				state.setSiteData ?
					<Panel
						state={state}
						action={{
							HandleEdit,
							EditMainValue,
							GetRebateInfo,
							GetTpGameTypeListbyCategory,
							GetRebateInfoByGameType,
							SetData,
							SetRebatePeriod,
							SetRebatePayoutLimit,
							SetRebateCategory,
							SetRebateGameType
						}}
						processing={processing}
						processArea={processArea}
					/> :
					null
			}
		</div>
	)
};

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}
