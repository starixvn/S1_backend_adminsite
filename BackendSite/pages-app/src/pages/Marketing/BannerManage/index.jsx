import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";
import Edit from "./Edit";
import qs from "qs";
import {
	defActionType,
	defPanelType,
	reducer,
	initState
} from "./index.reducer";
import Main from "./index.main";

const App = () => {
	const [state, dispatch] = useReducer(reducer, initState);
	const [processing, setProcessing] = useState(false);
	const [BannerType, setBannerType] = useState(0);
	const [Status, setStatus] = useState(0);
	const [Sitid, setSitid] = useState(0);
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		let param = {
			Url: "Common/AdminUserSiteList"
		};
		WebAPI.PostData(param).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.SET_SITE_LIST,
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
	}, []);

	const FilterOnchange=(v,dataColumn)=>{
		console.log(v,dataColumn);
		setBannerType(v);
		if (!processing && Sitid !== 0) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = "";
			if(dataColumn == "BannerType"){
				param = {
					Url: "BannerManage/GetBannerList",
					Data: qs.stringify({
						SiteId: Sitid,
						BannerTypeId:parseInt(v),
						Status:parseInt(Status)
					})
				};
			}
			else{
				param = {
					Url: "BannerManage/GetBannerList",
					Data: qs.stringify({
						SiteId: Sitid,
						BannerTypeId:parseInt(BannerType),
						Status:parseInt(v)
					})
				};
			}
			// let param = {
			// 	Url: "BannerManage/GetBannerList",
			// 	Data: qs.stringify({
			// 		SiteId: Sitid,
			// 		BannerTypeId:parseInt(v)
			// 	})
			// };
			let site = state.siteList.find(s => s.siteId === Sitid);
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_BANNER_LIST,
								bannerList: res.data.data,
								setSite: Sitid,
								setCurrencyList: site.iCurrencyList,
								setLangList: site.iLangList
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
						setSitid(Sitid);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
						setSitid(Sitid);
					}
				}
			);
		}
	}

	const GetSiteBannerList = siteId => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let param = {
				Url: "BannerManage/GetBannerList",
				Data: qs.stringify({
					SiteId: siteId,
					BannerTypeId:BannerType
				})
			};
			let site = state.siteList.find(s => s.siteId === siteId);
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.SET_BANNER_LIST,
								bannerList: res.data.data,
								setSite: siteId,
								setCurrencyList: site.iCurrencyList,
								setLangList: site.iLangList
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						setProcessing(false);
						setSitid(siteId);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						setProcessing(false);
						setSitid(siteId);
					}
				}
			);
		}
	}

	const ClickCreateBanner = () => {
		dispatch({ type: defActionType.GOTO_ADD_BANNER, });
	};

	const SetBanner = banner => {
		if (!processing) {
			if (isMountedRef.current) {
				setProcessing(true);
			}
			let _specific = Object.keys(banner.specific)
				.filter(c => {
					return banner.selectedCurrencies.includes(Number(c));
				})
				.map(s => {
					let info = banner.specific[s];
					let lang = {};
					info.selectedLang.forEach(l => {
						lang[l] = info.lang[l];
					});
					return {
						CurrencyId: s,
						Details: JSON.stringify(lang),
						Status: info.status
					};
				});

			let _banner = {
				SiteId: Number(banner.siteId),
				BannerName: banner.bannerName,
				BannerType: Number(banner.bannerType),
				Sort: Number(banner.sort),
				ActionMode: Number(banner.actionMode),
				ShowStatus: Number(banner.showStatus),
				UnixStartTime: moment(banner.startTime).unix(),
				UnixEndTime: moment(banner.endTime).unix(),
				BonusCode: banner.bonusCode,
				MatchId: Number(banner.matchId),
				TpId: Number(banner.tpId),
				GameId: banner.gameId,
				UrlRouter: banner.urlRouter,
				UnixSportBattleDateTime: moment(banner.sportBattleDateTime).unix(),
				SportCategoryId: Number(banner.sportCategoryId),
				SportLeaguesId: Number(banner.sportLeaguesId),
				SportTeamsId: (Object.keys(banner.sportTeamsId).length > 0) ? [{ "Home": Number(banner.sportTeamsId.home), "Away": Number(banner.sportTeamsId.away) }] : null,
				Specific: _specific
			};
			let param = {
				Url: "BannerManage/SetBanner",
				Data: qs.stringify({
					// IsAdd: state.panelType === defPanelType.Add,
					bannerInfo: _banner
				})
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode <= 2 ) {
							dispatch({ type: defActionType.SET_BANNER });
							GetSiteBannerList(_banner.SiteId);
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
			let bannerData = {};
			try {
				const res = await WebAPI.PostData({
					Url: "BannerManage/GetBanner",
					Data: qs.stringify({
						SiteId: state.bannerList[idx].siteId,
						BannerName: state.bannerList[idx].bannerName
					})
				})

				bannerData = {
					errorCode: res.data.errorCode,
					data: res.data.data,
					msg: res.data.message
				}

				if (bannerData.errorCode === 0) {
					if (isMountedRef.current) {
						setProcessing(false);
						dispatch({
							type: defActionType.EDIT_BANNER,
							bannerData: bannerData.data,
						});
					}

				} else {
					if (isMountedRef.current) {
						setProcessing(false);
					}
					let msg =
						bannerData.errorCode === 991 ? "API Server Error." : bannerData.msg;
					alert(msg);
				}
			} catch (error) {
				if (isMountedRef.current) {
					setProcessing(false);
				}
				bannerData = { errorCode: 991 }
			}

		}
	};

	const UpdateBannerStatus = idx => {
		let _banner = state.bannerList[idx];
		let msg =
			_banner.status === 1
				? "Disable this banner all currency?"
				: "Enable this banner all currency?";
		if (_banner && window.confirm(msg)) {
			let newStatus = _banner.status === 0 ? 1 : 0;
			let param = {
				Url: "BannerManage/UpdateBannerStatus",
				Data: qs.stringify({
					SiteId: _banner.siteId,
					BannerName: _banner.bannerName,
					Status: newStatus
				})
			};
			WebAPI.PostData(param).then(res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						GetSiteBannerList(_banner.siteId);
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

	const skipField = {
		"default": ["bonusCode", "matchId", "tpId", "gameId", "urlRouter", "sportBattleDateTime", "sportCategoryId", "sportLeaguesId", "sportTeamsId"],
		"2": ["bonusCode", "gameId", "urlRouter", "currentTab", "selectedCurrencies", "specific", "currentLang"],
	}

	return state.panelType === defPanelType.Main ? (
		<Main
			GetSiteBannerList={GetSiteBannerList}
			ClickCreateBanner={ClickCreateBanner}
			ClickEdit={ClickEdit}
			UpdateBannerStatus={UpdateBannerStatus}
			siteList={state.siteList}
			setSite={state.setSite}
			bannerList={state.bannerList}
			FilterOnchange={FilterOnchange}
			BannerType={BannerType}
			Status={Status}
		/>
	) : (
			<Edit
				Datas={state.bannerData}
				SetSite={state.setSite}
				SetCurrencyList={state.setCurrencyList}
				SetLangList={state.setLangList}
				action={{ SetBanner, ClickBack, setProcessing }}
				IsAdd={state.panelType === defPanelType.Add}
				processing={processing}
				skipField={skipField}
			/>
		);
};

export default App;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<App />, document.getElementById("content"));
}
