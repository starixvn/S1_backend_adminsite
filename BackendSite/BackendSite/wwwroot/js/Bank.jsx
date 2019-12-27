
class Header extends React.Component {

    render() {
        return (
            <thead>
                <tr>
                    <th scope="col">BankCode</th>
                    <th scope="col">BankName</th>
                    <th scope="col">AccountName</th>
                    <th scope="col">AccountNo</th>
                    <th scope="col">MinAmount</th>
                    <th scope="col">MaxAmount</th>
                    <th scope="col">Currency</th>
                    <th scope="col">Status</th>
                    <th scope="col"></th>
                </tr>
            </thead>
        );
    }
}


class Row extends React.Component {

    render() {
        let Data = this.props.Data;
        let currency = null;
        let status = null;
        switch(Data.currencyId) {
            case 2: currency = i18n.lbl_Currency2;
                     break;
            case 4: currency = i18n.lbl_Currency4;
                     break;
            case 15: currency = i18n.lbl_Currency15;
                     break;         
            case 51: currency = i18n.lbl_Currency51;
                     break;
        }
        switch(Data.status) {
            case 0: status = i18n.lbl_Disable;
                     break;
            case 1: status = i18n.lbl_Enable;
                     break;
        }
        return (
            <tr>
                <th scope="row">{Data.bankCode}</th>
                <td>{Data.bankName}</td>
                <td>{Data.accountName}</td>
                <td>{Data.accountNo}</td>
                <td>{Data.minAmount}</td>
                <td>{Data.maxAmount}</td>
                <td>{currency}</td>
                <td>{status}</td>
                <td>
                    <input type="button" class="btn btn-primary" onclick="UpdateStatus(1)" value="Edit" />
                </td>
            </tr>
        );
    }
}


class RowEdit extends React.Component {

    render() {
        let Data = this.props.Data;
        return (
            <tr>
                <td><input type="text" name="bankCode" value={Data.bankCode} /></td>
                <td><input type="text" name="bankName" value={Data.bankName} /></td>
                <td><input type="text" name="accountName" size="10" value={Data.accountName} /></td>
                <td><input type="text" name="accountNo" maxlength="20" size="20" value={Data.accountNo} /></td>
                <td><input type="text" name="minAmount" maxlength="4" size="4" value={Data.minAmount} onkeyup="value=value.replace(/[^\d]/g,'') " /></td>
                <td><input type="text" name="maxAmount" maxlength="4" size="4" value={Data.maxAmount} onkeyup="value=value.replace(/[^\d]/g,'') " /></td>
                <td><input type="text" name="currencyId" value={Data.currencyId} size="4" /></td>
                <td>
                    <div class="custom-control custom-switch">
                        <input type="checkbox" class="custom-control-input" id="customSwitches" disabled />
                    </div>
                </td>
                <td>
                    <input type="button" class="btn btn-primary" onclick="UpdateStatus(1)" value="Save" />
                    <input type="button" class="btn btn-primary" onclick="UpdateStatus(1)" value="Cancel" />
                </td>
            </tr>
        );
    }
}

class Tag extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Data: null };
    }

    componentDidMount() {
        let tt = this;
        axios.post('/api/BankList/GetBankListAll')
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    tt.setState({ Data: result.data });
                } else {
                    alert(result.message);
                }
            }).catch(function (error) {
                alert(error);
            });
    }

    render() {
        let rows = null;

        if (this.state.Data != null) {
            rows = this.state.Data.map(function (obj) {
                return <Row Data={obj} />;
            });

        }
        //;
        return (
            <table class="table table-striped">
                <Header />
                <tbody>
                    {rows}
                </tbody>
            </table>
        );
    }
}

ReactDOM.render(
    <Tag />,
    document.getElementById('Bank')
);