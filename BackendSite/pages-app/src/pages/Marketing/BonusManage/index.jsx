import React, { useEffect, useRef, useReducer } from "react";
import ReactDOM from 'react-dom'
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
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
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		let param = {
			Url: "BonusManage/GetPromotionList"
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
	}, []);

	const ClickCreateBonus = () => {
		let param = {
			Url: "BonusManage/GetSiteListByLoginUser"
		};
		WebAPI.PostData(param).then(res => {
			if (isMountedRef.current) {
				if (res.data.errorCode === 0) {
					dispatch({
						type: defActionType.GOTO_ADD_BONUS,
						siteList: res.data.data
					});
				} else {
					alert(res.data.message ?? "failed to get data.");
				}
			}
		});
	};

	const SetPromotion = promotion => {
		let _specific = Object.keys(promotion.specific).map(s => {
			return {
				CurrencyId: s,
				Usage: s.usage ? Number(s.usage) : 0,
				Rollover: Number(s.rollover),
				Bonus: Number(s.bonus),
				BonusType: s.bonusAmountType,
				MaxBonus: Number(s.maxBonus),
				Details: JSON.stringify({
					Title: s.title,
					BannerUrl: s.bannerUrl,
					Detail: s.detail
				}),
				Status: s.status
			};
		});

		let _promo = {
			BonusCode: promotion.bonusCode,
			Description: promotion.description,
			PromotionType: promotion.promotionType,
			UnixPeriodFrom: moment(promotion.periodFrom).unix(),
			UnixPeriodTo: moment(promotion.periodTo).unix(),
			Status: promotion.status,
			ExpiryDays: Number(promotion.expiryDays),
			SiteId: promotion.siteId,
			Specific: _specific
		};

		let param = {
			Url: "BonusManage/SetPromotion",
			Data: qs.stringify({
				IsAdd: state.panelType === defPanelType.Add,
				Promotion: _promo
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
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
				}
			},
			err => {
				alert("API Server Error.");
			}
		);
	};

	const ClickEdit = async idx => {
		const promotionData = await WebAPI.PostData({
			Url: "BonusManage/GetPromotion",
			Data: qs.stringify({
				BonusCode: state.promotionList[idx].bonusCode
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
			Data: qs.stringify({ SiteId: state.promotionList[idx].siteId })
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
				promotionData.errorCode === 991 && siteList.errorCode === 991
					? "API Server Error."
					: [promotionData.msg, siteList.msg].join(", ");
			alert(msg);
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
							status: newStatus
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

	return state.panelType === defPanelType.Main ? (
		<Main
			ClickCreateBonus={ClickCreateBonus}
			ClickEdit={ClickEdit}
			UpdatePromotionStatus={UpdatePromotionStatus}
			promotionList={state.promotionList}
		/>
	) : (
		<Edit
			Datas={state.promotionData}
			SiteList={state.siteList}
            action={{ SetPromotion, ClickBack }}
            IsAdd={state.panelType === defPanelType.Add}
		/>
	);
};

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}