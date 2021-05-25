import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import Edit from "./Edit";
import ScratchList from "./ScratchList";
import qs from "qs";
import {
	defActionType,
	defPanelType,
	reducer,
	initState
} from "./index.reducer";
import Main from "./index.main";
import { initPromotionType } from "./defCols";

const App = () => {
	const [state, dispatch] = useReducer(reducer, initState);
	const [processing, setProcessing] = useState(false);
	const [stateFilter, setFilter] = useState({
		'SelectSite': ['checkStatus']
	});
	const [sitnemaFilter, setSitnameFilter] = useState([]);
	const [promotionTypeFilter, setPromotionTypeFilter] = useState([]);
	const [statusCheck, setStatusCheck] = useState([]);

	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	const GetInitSelectedSiteName = (props) => {
		let headerRows = [];
		props.map(x => {
			headerRows.push({
				label: x.siteName,
				value: x.siteId,
			})
		})
		return headerRows;
	};
	const GetInitSelectedPromotionType = (props) => {
		let headerRows = [];
		props.map(x => {
			headerRows.push({
				label: x.displayText,
				value: x.value,
			})
		})
		return headerRows;
	};

	useEffect(() => {
		let param1 = {
			Url: "Common/AdminUserSiteList"
		};
		let aaa = [];
		WebAPI.PostData(param1).then(x => {
			if (isMountedRef.current) {
				if (x.data.errorCode === 0) {
					let initSitnemaFilter = GetInitSelectedSiteName(x.data.data);
					let intPromotionTypeFilter = GetInitSelectedPromotionType(initPromotionType);
					setSitnameFilter(initSitnemaFilter);
					setPromotionTypeFilter(initPromotionType);
					let param = {
						Url: "BonusManage/GetPromotionList",
						Data: qs.stringify({
							SiteidFilter: JSON.stringify(initSitnemaFilter),
							PromotionTypeFilter: JSON.stringify(intPromotionTypeFilter),
							Status: statusCheck
						})
					};
					WebAPI.PostData(param).then(res => {
						if (isMountedRef.current) {
							if (res.data.errorCode === 0) {
								dispatch({
									type: defActionType.SET_PROMOTION_LIST,
									promotionList: res.data.data
								});
							} else {
								alert(res.data.message ?? "failed to get data.");
							}
						}
					});
				} else {
					alert(x.data.message ?? "failed to get data.");
				}
			}
		});


	}, []);

	useEffect(() => {
		let param = {
			Url: "Common/AdminUserSiteList"
		};
		WebAPI.PostData(param).then(res => {
			if (isMountedRef.current) {
				if (res.data.errorCode === 0) {
					dispatch({
						type: defActionType.SET_SITE_LIST,
						siteList: res.data.data
					});
				} else {
					alert(res.data.message ?? "failed to get data.");
				}
			}
		});
	}, []);

	const ClickCreateBonus = () => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: "BonusManage/GetSiteListByLoginUser"
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.GOTO_ADD_BONUS,
								siteList: res.data.data
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
	};

	const SetPromotion = promotion => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let _specific = Object.keys(promotion.specific)
				.filter(c => {
					return promotion.selectedCurrencies.includes(Number(c));
				})
				.map(s => {
					let info = promotion.specific[s];
					let lang = {};
					info.selectedLang.forEach(l => {
						lang[l] = info.lang[l];
					});
					let maxBonus =
						promotion.promotionType == 3
							? Number(info.bonus)
							: Number(info.maxBonus);

					let tpGame = promotion.promotionType == 6 || promotion.promotionType == 7 ? promotion.tpGameCategory.map(t => {
						return {
							GameType: t.value,
							Percentage: info.tpGames[t.value]?.percentage ? Number(info.tpGames[t.value].percentage) : 0,
							Status: info.tpGames[t.value]?.status ? Number(info.tpGames[t.value].status) : 0,
						}
					}) : null;

					return {
						CurrencyId: s,
						Usage: info.usage ? Number(info.usage) : 0,
						Rollover: Number(info.rollOver),
						Bonus: Number(info.bonus),
						BonusType: Number(info.bonusAmountType),
						MaxBonus: maxBonus,
						Details: JSON.stringify(lang),
						Status: info.status,
						RequiredAmount: Number(info.requiredAmount),
						LockedType: info.lockedType,
						BeenUsed: info.beenUsed,
						RebateList: tpGame,
						MinPayout: Number(info.minPayout),
						VN_AffCodes: info.affid,
						TotalUsageLimit: info.totalUsageLimit
					};
				});

			let _promo = {
				BonusCode: promotion.bonusCode,
				Description: promotion.description,
				PromotionType: promotion.promotionType,
				PromotionAction: promotion.promotionAction,
				PromotionDeviceType: promotion.promotionDeviceType,
				UnixPeriodFrom: moment(promotion.periodFrom).unix(),
				UnixPeriodTo: moment(promotion.periodTo).unix(),
				Status: promotion.status,
				ExpiryDays: Number(promotion.expiryDays),
				SiteId: promotion.siteId,
				Sort: promotion.sort,
				WalletType: promotion.walletType,
				ProductCategory: promotion.productCategory,
				TpId: promotion.tpId,
				CategoryId: Number(promotion.categoryId),
				Specific: _specific,
				PeriodType: Number(promotion.periodType) ? Number(promotion.periodType) : 0,
				PeriodDay: Number(promotion.periodType) === 2 ? Number(promotion.periodWeek) : Number(promotion.periodType) === 3 ? Number(promotion.periodMonth) : 0,
				RedeemHours: Number(promotion.redeemHours) ? Number(promotion.redeemHours) : 0,
				SpecialCode: promotion.specialCode,
				IsSpecial: promotion.isSpecial,
				OnlyRegistered: promotion.onlyRegistered.length > 0 ? true : false,
				AutoExtend: promotion.autoExtend.length > 0 ? 1 : 0
			};
			let _select = {
				SiteidFilter: JSON.stringify(sitnemaFilter),
				PromotionTypeFilter: JSON.stringify(promotionTypeFilter),
				Status: statusCheck.length > 0
			};
			let param = {
				Url: "BonusManage/SetPromotion",
				Data: qs.stringify({
					IsAdd: state.panelType === defPanelType.Add,
					Promotion: _promo,
					Select: _select
				})
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_PROMOTION,
								promotionList: res.data.data
							});
						} else if (res.data.errorCode === 1062) {
							alert("Duplicate Bonus Code.");
						} else {
							alert(res.data.message ?? "failed to set data.");
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
	};
	const ClickEdit = async idx => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			const promotionData = await WebAPI.PostData({
				Url: "BonusManage/GetPromotion",
				Data: qs.stringify({
					BonusCode: state.promotionList[idx].bonusCode,
					SiteId: state.promotionList[idx].siteId
				})
			}).then(
				res => {
					return {
						errorCode: res.data.errorCode,
						data: res.data.data,
						msg: res.data.message
					};
				},
				err => {
					return { errorCode: 991 };
				}
			);

			const siteList = await WebAPI.PostData({
				Url: "BonusManage/GetSiteListByLoginUser",
				Data: qs.stringify({})
			}).then(
				res => {
					return {
						errorCode: res.data.errorCode,
						data: res.data.data,
						msg: res.data.message
					};
				},
				err => {
					return { errorCode: 991 };
				}
			);
			if (promotionData.errorCode === 0 && siteList.errorCode === 0) {
				dispatch({
					type: defActionType.EDIT_PROMOTION,
					promotionData: promotionData.data,
					siteList: siteList.data
				});
			} else {
				let msg =
					promotionData.errorCode === 991 &&
						siteList.errorCode === 991
						? "API Server Error."
						: [promotionData.msg, siteList.msg].join(", ");
				alert(msg);
			}

			if (isMountedRef.current) {
				setProcessing(false);
			}
		}
	};

	const ClickEdit2 = async idx => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			const scratchCardData = await WebAPI.PostData({
				Url: "BonusManage/GetScratchCardList",
				Data: {
					SpecialCode: state.promotionList[idx].specialCode,
				}
			}).then(
				res => {
					return {
						errorCode: res.data.errorCode,
						data: res.data.data,
						msg: res.data.message
					};
				},
				err => {
					return { errorCode: 991 };
				}
			);
			if (scratchCardData.errorCode === 0) {
				dispatch({
					type: defActionType.SCRATCH_CARD_LIST,
					scratchCardData: scratchCardData.data,
					specialCode: state.promotionList[idx].specialCode,
					popBoxIsOpenGen: false,
			        genNum: 0
				});
			} else {
				let msg =
				scratchCardData.errorCode === 991 
						? "API Server Error."
						: scratchCardData.msg;
				alert(msg);
			}

			if (isMountedRef.current) {
				setProcessing(false);
			}
		}
	};
	const UpdatePromotionStatus = idx => {
		let _promo = state.promotionList[idx];
		let msg =
			_promo.status === 1
				? "Disable this promotion?"
				: "Enable this promotion?";
		if (_promo && window.confirm(msg)) {
			let newStatus = _promo.status === 1 ? 2 : 1;
			let param = {
				Url: "BonusManage/UpdatePromotionStatus",
				Data: qs.stringify({
					BonusCode: _promo.bonusCode,
					SiteId: _promo.siteId,
					Status: newStatus
				})
			};
			WebAPI.PostData(param).then(res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.UPDATE_PROMOTION_STATUS,
							idx: idx,
							status: newStatus,
							lastUpdateUser: res.data.data.lastUpdateUser,
							lastUpdateTime: res.data.data.lastUpdateTime
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
				}
			});
		}
	};

	const ClickBack = () => {
		dispatch({ type: defActionType.CLICK_BACK });
	};

	const ClickClone = async idx => {
		dispatch({
			type: defActionType.HANDEL_POPBOX,
			popBoxIsOpen: true,
			promotionData: state.promotionList[idx],
			clonePromotionData: {
				bonusCode: state.promotionList[idx].bonusCode,
				siteId: state.promotionList[idx].siteId
			}
		});
	};
	const ClickGen= async idx => {
		dispatch({
			type: defActionType.HANDEL_POPBOXGEN,
			popBoxIsOpenGen: true,
			genNum: 0
			
		});
	};

	const SetCloneValue = (value, dataColumn) => {
		dispatch({ type: defActionType.SET_CLONE_VALUE, value, dataColumn });
	};
	const SetGenNumValue = (value) => {
		dispatch({ type: defActionType.SET_GENNUM_VALUE, value });
	};

	const ClickCancle = () => {
		dispatch({
			type: defActionType.HANDEL_POPBOX,
			popBoxIsOpen: false,
			promotionData: []
		});
	};
	const ClickGenCancle = () => {
		dispatch({
			type: defActionType.HANDEL_POPBOXGEN,
			popBoxIsOpenGen: false,
			genNum: 0
		});
	};

	const SubmitClone = () => {
		if (!processing && state.clonePromotionData.cloneBonusCode) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: "BonusManage/DuplicatePromotion",
				Data: {
					BonusCode: state.clonePromotionData.bonusCode,
					SiteId: state.clonePromotionData.siteId,
					NewBonusCode: state.clonePromotionData.cloneBonusCode,
				}
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_PROMOTION,
								promotionList: res.data.data,
								popBoxIsOpen: false,
							});
						} else if (res.data.errorCode === 1062) {
							alert("Duplicate Bonus Code.");
						} else {
							alert(res.data.message ?? "failed to set data.");
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
	};
	const GenerarStart = () => {
		if (!processing && state.specialCode && state.genNum > 0) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: "BonusManage/GenScratchCardCode",
				Data: {
					SpecialCode: state.specialCode,
					Num: state.genNum
				}
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							let param = {
								Url: "BonusManage/GetScratchCardList",
								Data: {
									SpecialCode: state.specialCode,
								}
							};
							WebAPI.PostData(param).then(
								res => {
									if (isMountedRef.current) {
										if (res.data.errorCode === 0) {
											dispatch({
												type: defActionType.SCRATCH_CARD_LIST,
												scratchCardData: res.data.data,
												specialCode: state.specialCode,
												popBoxIsOpenGen: false,
												genNum: 0
											});
										} else {
											alert(res.data.message ?? "failed to get new ScratchCard List.");
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
						} else {
							alert(res.data.message ?? "failed to Generar ScratchCardCode.");
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
		}else if(state.genNum <=0)
		{
			alert("Number cannot be zero");
	    }
	};
    
	const StateCheckBox = (value, dataColumn) => {
		if (statusCheck.length > 0) {
			setStatusCheck([])
		}
		else {
			setStatusCheck([...statusCheck, value])
		}
	};

	const SelectClick = () => {
		let param = {
			Url: "BonusManage/GetPromotionList",
			Data: qs.stringify({
				SiteidFilter: JSON.stringify(sitnemaFilter),
				PromotionTypeFilter: JSON.stringify(promotionTypeFilter),
				Status: statusCheck.length > 0
			})
		};
		WebAPI.PostData(param).then(res => {
			if (isMountedRef.current) {
				if (res.data.errorCode === 0) {
					dispatch({
						type: defActionType.SET_PROMOTION_LIST,
						promotionList: res.data.data
					});
				} else {
					alert(res.data.message ?? "failed to get data.");
				}
			}
		});

	};

	const skipField = {
		"default": ["currentLang", "productCategory", "lockedType", "periodWeek", "periodMonth", "redeemHours", "minPayout", "specialCode", "promotionAction",""],
		"6": ["expiryDays", "walletType", "categoryId", "tpId", "bonusAmountType", "bonus", "maxBonus", "requiredAmount", "currentLang", "productCategory", "lockedType", "affid", "specialCode", "promotionAction"],
		"7": ["expiryDays", "walletType", "categoryId", "tpId", "bonusAmountType", "bonus", "maxBonus", "requiredAmount", "currentLang", "productCategory", "lockedType", "affid", "specialCode", "promotionAction"],
		"8": ["expiryDays", "walletType", "categoryId", "tpId", "rollOver", "minPayout", "bonusAmountType", "bonus", "maxBonus", "requiredAmount", "currentLang", "productCategory", "lockedType", "periodWeek", "periodMonth", "redeemHours", "affid", "specialCode"],
	}

	if(state.panelType === defPanelType.Main || state.panelType === defPanelType.Copy )
	{
		return (
			<Main
				ClickCreateBonus={ClickCreateBonus}
				ClickEdit={ClickEdit}
				ClickEdit2={ClickEdit2}
				UpdatePromotionStatus={UpdatePromotionStatus}
				SelectClick={SelectClick}
				promotionList={state.promotionList}
				Datas={state.promotionData}
				NewDatas={state.clonePromotionData}
				Action={{ ClickClone, ClickCancle, SetCloneValue, SubmitClone, ClickGen, ClickGenCancle, GenerarStart, SetGenNumValue}}
				IsOpen={state.popBoxIsOpen}
				SiteList={state.siteList}
				StateFilter={stateFilter}
				SitnemaFilter={sitnemaFilter}
				PromotionTypeFilter={promotionTypeFilter}
				StatusCheck={statusCheck}
				SetSitnemaFilter={setSitnameFilter}
				SetPromotionTypeFilter={setPromotionTypeFilter}
				StateCheckBox={StateCheckBox}
				IsOpenGen={state.popBoxIsOpenGen}
			/>
		) ;
	} else if(state.panelType === defPanelType.SList)
	{
		return (
			<ScratchList
				Datas={state.scratchCardData}
				action={{ ClickBack, ClickGen, GenerarStart, ClickGen, ClickGenCancle, GenerarStart, SetGenNumValue}}
				processing={processing}
				skipField={skipField}
				IsOpenGen={state.popBoxIsOpenGen}
			/>
		);
	}else 
	{
		return (
			<Edit
				Datas={state.promotionData}
				SiteList={state.siteList}
				action={{ SetPromotion, ClickBack }}
				IsAdd={state.panelType === defPanelType.Add}
				processing={processing}
				skipField={skipField}
			/>
		);
	}



	// return state.panelType === defPanelType.Main || state.panelType === defPanelType.Copy ? (
	// 	<Main
	// 		ClickCreateBonus={ClickCreateBonus}
	// 		ClickEdit={ClickEdit}
	// 		ClickEdit2={ClickEdit2}
	// 		UpdatePromotionStatus={UpdatePromotionStatus}
	// 		SelectClick={SelectClick}
	// 		promotionList={state.promotionList}
	// 		Datas={state.promotionData}
	// 		NewDatas={state.clonePromotionData}
	// 		Action={{ ClickClone, ClickCancle, SetCloneValue, SubmitClone }}
	// 		IsOpen={state.popBoxIsOpen}
	// 		SiteList={state.siteList}
	// 		StateFilter={stateFilter}
	// 		SitnemaFilter={sitnemaFilter}
	// 		PromotionTypeFilter={promotionTypeFilter}
	// 		StatusCheck={statusCheck}
	// 		SetSitnemaFilter={setSitnameFilter}
	// 		SetPromotionTypeFilter={setPromotionTypeFilter}
	// 		StateCheckBox={StateCheckBox}
	// 	/>
	// ) : (
	// 	<Edit
	// 		Datas={state.promotionData}
	// 		SiteList={state.siteList}
	// 		action={{ SetPromotion, ClickBack }}
	// 		IsAdd={state.panelType === defPanelType.Add}
	// 		processing={processing}
	// 		skipField={skipField}
	// 	/>
	// );
};

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}
