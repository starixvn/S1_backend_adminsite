import React from 'react'
import { Dropdown, Button, Icon, Table, Form } from 'semantic-ui-react'
import { render } from 'react-dom';
import LoadingOverlay from 'react-loading-overlay';
import Swal from 'sweetalert2';
import withReactContent from 'sweetalert2-react-content';
import { number_format } from '../commJS/common';
const Toastr = withReactContent(Swal);

const showError = message => {
    Toastr.fire({
        icon: 'error',
        title: i18n.lbl_Error,
        text: message,
        allowOutsideClick: false,
        animation: false,
        allowEnterKey: false
    });
};
const showSuccess = message => {
    Toastr.fire({
        icon: 'success',
        title: i18n.lbl_Success,
        text: message,
        allowOutsideClick: false,
        animation: false,
        allowEnterKey: false
    });
};

const DropdownSelection = ({ options, label, isLoading, onChange, value }) => {
    return (
        <Dropdown
            placeholder={label}
            selection
            options={options}
            loading={isLoading}
            onChange={onChange}
            value={value}
        />
    );
}

const getPointLevels = async (siteId, currencyId) => {
    const response = await axios.post('/api/Marketing/GetPointLevelInfos', { siteId: siteId, currencyId: currencyId });
    try {
        const result = response.data;
        if (result.errorCode == 0) {
            const points = result.data;
            var pointLevels = points.map(point => {
                var rObj = {};
                rObj["pointLevel"] = point.pointLevel;
                rObj["depositLeast"] = point.depositLeast;
                rObj["betLeast"] = point.betLeast;
                rObj["isEditing"] = false;
                return rObj;
            });
            return pointLevels;
        } else {
            alert(result.message);
            return [];
        }
    } catch (error) {
        alert(error);
        return [];
    }
}

class Popup extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            deposit: props.minDepositLeast + 1,
            bet: props.minBetLeast + 1
        }
        this.onChangeDepositLeast = this.onChangeDepositLeast.bind(this);
        this.onChangeBetLeast = this.onChangeBetLeast.bind(this);
        this.onSave = this.onSave.bind(this);
        this.onBlurBetLeast = this.onBlurBetLeast.bind(this);
        this.onBlurDepositLeast = this.onBlurDepositLeast.bind(this);
    }

    onBlurBetLeast(e) {
        if (e.target.value >= this.props.minBetLeast + 1) {
            this.setState({
                bet: parseInt(e.target.value)
            });
        }
    }

    onBlurDepositLeast(e) {
        if (e.target.value >= this.props.minDepositLeast + 1) {
            this.setState({
                deposit: parseInt(e.target.value)
            });
        }
    }

    onChangeBetLeast(e) {
        this.setState({
            bet: parseInt(e.target.value)
        });
    }

    onChangeDepositLeast(e) {
        this.setState({
            deposit: parseInt(e.target.value)
        });
    }

    onSave() {
        this.props.onClickSaveNewPoint(this.props.point, this.state.deposit, this.state.bet);
    }

    render() {
        return (
            <div className='popup'>
                <div className='popup_inner'>
                    <h1>{this.props.text}</h1>
                    <Form>
                        <Form.Field>
                            <label>{i18n.lbl_Point}</label>
                            <input value={this.props.point} readOnly />
                        </Form.Field>
                        <Form.Field>
                            <label>{i18n.lbl_DepositLeast}</label>
                            <input
                                value={this.state.deposit}
                                type="number"
                                onChange={(event) => this.onChangeDepositLeast(event)}
                            />
                        </Form.Field>
                        <Form.Field>
                            <label>{i18n.lbl_BetLeast}</label>
                            <input
                                value={this.state.bet}
                                type="number"
                                onChange={(event) => this.onChangeBetLeast(event)}
                            />
                        </Form.Field>
                        <button
                            className="ui right floated primary button"
                            onClick={this.onSave}>{i18n.lbl_Save}
                        </button>
                        <button
                            className="ui right floated button"
                            onClick={this.props.closePopup}>{i18n.lbl_Cancel}
                        </button>
                        <div className="clearfix" />
                    </Form>
                </div>
            </div>
        );
    }
}

class EditableRow extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isEditing: false,
            deposit: props.rowData.depositLeast,
            bet: props.rowData.betLeast,
            isChanged: false
        }
        this.onClickEditRow = this.onClickEditRow.bind(this);
        this.onSaveEditedRow = this.onSaveEditedRow.bind(this);
        this.onChangeValue = this.onChangeValue.bind(this);
        this.onCancel = this.onCancel.bind(this);
    }

    async onSaveEditedRow() {
        const { deposit, bet } = this.state;
        var result = await this.props.onSaveEditedRow(this.props.rowData.pointLevel, deposit, bet);

        if (result) {
            this.setState({
                isEditing: false,
                isChanged: false
            });
        } else {
            this.setState({
                deposit: this.props.rowData.depositLeast,
                bet: this.props.rowData.betLeast,
                isEditing: false,
                isChanged: false
            });
        }
    }

    onClickEditRow() {
        this.setState({
            isEditing: true
        });
    }

    onChangeValue = event => {
        const target = event.target;
        const name = target.name;
        let value = target.value;

        if (value < 0) {
            value = 0;
        }

        if (value !== this.state[name]) {
            this.setState({
                [name]: value,
                isChanged: true
            });
        }
    }

    onCancel() {
        this.setState({
            isEditing: false,
            deposit: this.props.rowData.depositLeast,
            bet: this.props.rowData.betLeast,
            isChanged: false
        });
    }

    render() {
        const { isChanged, isEditing, deposit, bet } = this.state;
        return (
            <Table.Row className="point">
                <Table.Cell className="text-center">{this.props.rowData.pointLevel}</Table.Cell>
                <Table.Cell className="text-right">{!isEditing && number_format(deposit)}{isEditing && <input type="number" className="input-full" onChange={e => this.onChangeValue({ target: { name: "deposit", value: parseInt(e.target.value) } })}
                    value={deposit}
                />}</Table.Cell>
                <Table.Cell className="text-right">{!isEditing && number_format(bet)}{isEditing && <input type="number" className="input-full" onChange={e => this.onChangeValue({ target: { name: "bet", value: parseInt(e.target.value) } })}
                    value={bet}
                />}</Table.Cell>
                <Table.Cell>{!isEditing && <div><Button
                    icon
                    primary
                    size='mini'
                    onClick={() => this.onClickEditRow()}
                > <Icon name='edit' /></Button>
                    {this.props.isDeletable && <Button
                        icon
                        color="red"
                        size='mini'
                        onClick={() => this.props.onDeletePoint(this.props.rowData.pointLevel)}
                    > <Icon name='trash alternate outline' /> </Button>}</div>}
                    {isEditing && <div><Button
                        icon
                        positive
                        size='mini'
                        disabled={!isChanged}
                        onClick={() => this.onSaveEditedRow()}
                    > <Icon name='check' /></Button>
                        <Button
                            icon
                            color="red"
                            size='mini'
                            onClick={() => this.onCancel()}
                        > <Icon name='cancel' /> </Button>
                    </div>}
                </Table.Cell>
            </Table.Row>
        );
    }
}

class PointTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            points: [],
            isLoading: false,
            isAddNew: false
        }
        this.onClickOpenAddNewPopUp = this.onClickOpenAddNewPopUp.bind(this);
        this.onClickSaveNewPoint = this.onClickSaveNewPoint.bind(this);
        this.onSaveEditedRow = this.onSaveEditedRow.bind(this);
        this.onDeletePoint = this.onDeletePoint.bind(this);
    }

    async componentDidMount() {
        this.setState({ isLoading: true });
        const points = await getPointLevels(this.props.siteId, this.props.currencyId);
        this.setState({ points: points, isLoading: false });
    }

    onClickOpenAddNewPopUp = async (e) => {
        e.preventDefault();
        this.setState({
            isAddNew: !this.state.isAddNew
        });
    }

    onClickSaveNewPoint = async (point, deposit, bet) => {
        const { points } = this.state;

        var response = await axios.post("/api/Marketing/AddPointLevelInfo", { siteId: this.props.siteId, currencyId: this.props.currencyId, pointLevel: point, depositLeast: deposit, betLeast: bet });
        try {
            const result = response.data;
            if (result.errorCode == 0) {
                var rObj = {};
                rObj["pointLevel"] = point;
                rObj["depositLeast"] = deposit;
                rObj["betLeast"] = bet;
                rObj["isEditing"] = false;
                points.push(rObj);

                this.setState({ points, isAddNew: !this.state.isAddNew });
                showSuccess(result.message);
            } else {
                showError(result.message);
            }
        } catch (error) {
            showError(error);
        }
    }

    onDeletePoint = async (point) => {
        Toastr.fire({
            title: i18n.lbl_AreYouSure,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: 'red',
            cancelButtonColor: 'blue',
            confirmButtonText: i18n.lbl_OK
        }).then(async (result) => {
            if (result.value) {
                const { points } = this.state;
                var response = await axios.post("/api/Marketing/DeletePointLevelInfo", { siteId: this.props.siteId, currencyId: this.props.currencyId, pointLevel: point });
                try {
                    const result = response.data;
                    if (result.errorCode == 0) {
                        points.pop();
                        this.setState({ points });
                        showSuccess(result.message);
                    } else {
                        showError(result.message);
                    }
                } catch (error) {
                    showError(error);
                }
            }
        });
    }

    onSaveEditedRow = async (editingPoint, deposit, bet) => {
        const points = [...this.state.points];
        var index = points.findIndex(point => point.pointLevel === editingPoint);

        var response = await axios.post("/api/Marketing/EditPointLevelInfo", { siteId: this.props.siteId, currencyId: this.props.currencyId, pointLevel: editingPoint, depositLeast: deposit, betLeast: bet });
        try {
            const result = response.data;
            if (result.errorCode == 0) {
                points[index].depositLeast = deposit;
                points[index].betLeast = bet;
                this.setState({ points });
                showSuccess(result.message);
                return true;
            } else {
                showError(result.message);
                return false;
            }
        } catch (error) {
            showError(error);
            return false;
        }
    }

    render() {
        const { points } = this.state;
        const pointLevels = points.map(point => point.pointLevel);
        const currentMaxPoint = pointLevels.length > 0 ? Math.max(...pointLevels) : 0;

        const currentSecondMaxPointBet = currentMaxPoint > 0 ? points.filter(x => x.pointLevel === currentMaxPoint)[0].betLeast : 0;
        const currentSecondMaxPointDeposit = currentMaxPoint > 0 ? points.filter(x => x.pointLevel === currentMaxPoint)[0].depositLeast : 0;

        return (
            <div className="panel">
                <a className="panel-title" data-toggle="collapse" href={"#collapseExample-" + this.props.currencyId} role="button" aria-expanded="true" aria-controls="collapseExample">
                    {i18n["lbl_Currency" + this.props.currencyId]}
                </a>
                <div className="panel-body">
                    <div className="collapse show" id={"collapseExample-" + this.props.currencyId}>
                        <LoadingOverlay
                            active={this.state.isLoading}
                            spinner
                        >
                            <div className="point-table" data-currency-id={this.props.currencyId}>
                                <button
                                    className="btn btn-add"
                                    onClick={(e) => this.onClickOpenAddNewPopUp(e)}
                                >{i18n.lbl_AddPointLevel}
                                </button>
                                <Table compact celled>
                                    <Table.Header>
                                        <Table.Row>
                                            <Table.HeaderCell className="width-100 text-center">{i18n.lbl_Point}</Table.HeaderCell>
                                            <Table.HeaderCell>{i18n.lbl_DepositLeast}</Table.HeaderCell>
                                            <Table.HeaderCell>{i18n.lbl_BetLeast}</Table.HeaderCell>
                                            <Table.HeaderCell className="width-100">{i18n.lbl_Action}</Table.HeaderCell>
                                        </Table.Row>
                                    </Table.Header>

                                    <Table.Body>
                                        {
                                            points.map(rowData => <EditableRow key={rowData.pointLevel} rowData={rowData} onDeletePoint={this.onDeletePoint} onSaveEditedRow={this.onSaveEditedRow} isDeletable={currentMaxPoint > 0 && currentMaxPoint === rowData.pointLevel} />)
                                        }
                                    </Table.Body>
                                </Table>
                            </div>
                            {
                                this.state.isAddNew ?
                                    <Popup
                                        text={i18n.lbl_AddNewPointLevel}
                                        closePopup={this.onClickOpenAddNewPopUp}
                                        siteId={this.props.siteId}
                                        point={currentMaxPoint + 1}
                                        currencyId={this.props.currencyId}
                                        minBetLeast={currentSecondMaxPointBet}
                                        minDepositLeast={currentSecondMaxPointDeposit}
                                        onClickSaveNewPoint={this.onClickSaveNewPoint}
                                    />
                                    : null
                            }
                        </LoadingOverlay>
                    </div>
                </div>
            </div>
        );
    }
}

class PointLevelManagement extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            sites: [],
            isLoadingSite: false,
            selectedSite: null,
            currencyOptions: []
        }
        this.onChangeSite = this.onChangeSite.bind(this);
    }

    componentDidMount() {
        this.setState({ isLoadingSite: true });
        axios.get('/api/Marketing/GetAllSites')
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    var serverInfos = result.data;
                    var sites = serverInfos.map(info => {
                        var rObj = {};
                        rObj["key"] = info.siteId;
                        rObj["value"] = info.siteId;
                        rObj["text"] = info.siteName;
                        return rObj;
                    });
                    this.setState({ sites: sites, isLoadingSite: false });
                } else {
                    showError(result.message);
                }
            }.bind(this)).catch(function (error) {
                showError(error);
                this.setState({ isLoadingSite: false });
            }.bind(this)).finally(function () {
            }.bind(this));
    }

    onChangeSite = async (e, { value }) => {
        this.setState({ selectedSite: value, currencyOptions: [] });
        const response = await axios.post('/api/Marketing/GetSiteCurrencyIds', { siteId: value });
        try {
            const result = response.data;
            if (result.errorCode == 0) {
                const currencyIds = result.data;
                var currencyOptions = currencyIds.map(currency => {
                    var rObj = {};
                    rObj["key"] = currency;
                    rObj["value"] = currency;
                    rObj["text"] = currency;
                    return rObj;
                });
                this.setState({ currencyOptions: currencyOptions });
            } else {
                alert(result.message);
            }
        } catch (error) {
            alert(error);
        }
    }

    render() {
        const { sites, selectedSite, currencyOptions, isLoadingSite } = this.state;
        return (
            <div className="pt-15">
                <div className="filter">
                    <Form.Field className="input-item">
                        <label className="label">{i18n.lbl_SelectSite}</label>
                        <DropdownSelection options={sites} label="Select site" onChange={this.onChangeSite} value={selectedSite} isLoading={isLoadingSite} />
                    </Form.Field>
                </div>
                <div className="panel-flex">
                    {selectedSite && (currencyOptions.map(option => <div key={option.value} className="form-group">
                        <PointTable currencyId={option.value} siteId={selectedSite} />
                    </div>))}
                </div>
            </div>
        );
    }
}

render(<PointLevelManagement />, document.getElementById('point-management'));